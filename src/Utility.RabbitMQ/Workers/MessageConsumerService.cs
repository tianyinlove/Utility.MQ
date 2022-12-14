using Utility.RabbitMQ.Constants;
using Utility.Extensions;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using Utility.RabbitMQ.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net.NetworkInformation;

namespace Utility.RabbitMQ;

/// <summary>
/// 
/// </summary>
internal class MessageConsumerService<TComsumer> : BackgroundService
{
    #region 变量和ioc

    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// MQ配置，Json格式
    /// </summary>
    public string RabbitMQConfig { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Type MessageType { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    public Type ConsumerType { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    public ushort PrefetchCount { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    public ushort PrefetchSize { get; internal set; }

    /// <summary>
    /// 消费者应用id
    /// </summary>
    private string ProducerAppId { get; set; }

    /// <summary>
    /// 消息路由
    /// </summary>
    private string RoutingKey { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string QueueName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool AutoDelete { get; set; }

    /// <summary>
    /// ioc
    /// </summary>
    public MessageConsumerService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        ParseSettings();
    }

    /// <summary>
    /// 获取消费者相关设置
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    private void ParseSettings()
    {
        ConsumerType = typeof(TComsumer);
        if (ConsumerType.BaseType.IsGenericType && ConsumerType.BaseType.GetGenericTypeDefinition() == typeof(MessageConsumer<>))
        {
            MessageType = ConsumerType.BaseType.GenericTypeArguments[0];
        }
        if (MessageType == null)
        {
            throw new ArgumentNullException($"{nameof(TComsumer)}未实现MessageConsumer<>");
        }

        if (MessageType.GetCustomAttribute<RabbitMQAttribute>() is RabbitMQAttribute keyAttribute
            && !string.IsNullOrWhiteSpace(keyAttribute.RouteKey))
        {
            RoutingKey = keyAttribute.RouteKey;
            ProducerAppId = keyAttribute.AppId;
        }
        else
        {
            throw new ArgumentNullException("未标记RabbitMQAttribute并设置routingkey");
        }
        using var scope = _scopeFactory.CreateScope();
        var settings = scope.ServiceProvider.GetRequiredService(ConsumerType) as IMessageConsumer;

        RabbitMQConfig = settings.RabbitMQConfig;
        AutoDelete = settings.AutoDelete;

        if (string.IsNullOrEmpty(RabbitMQConfig))
        {
            if (string.IsNullOrEmpty(keyAttribute.MqName))
            {
                throw new ArgumentNullException($"无法识别MQ名，请通过{nameof(RabbitMQAttribute)}标注或赋值RabbitMQConfig");
            }
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            RabbitMQConfig = configuration.GetSection(keyAttribute.MqName).Value;
            if (string.IsNullOrEmpty(RabbitMQConfig))
            {
                throw new ArgumentNullException($"{keyAttribute.MqName}未配置，请配置appsettings.json或赋值RabbitMQConfig");
            }
        }

        QueueName = settings.ConsumerAppId == ProducerAppId
            ? $"{settings.ConsumerName}.{RoutingKey}"
            : $"{settings.ConsumerAppId.ToString().ToLower()}.{settings.ConsumerName}.{RoutingKey}";

        if (AutoDelete)
        {
            QueueName += $".{GetMac().Md5()}";
        }

        PrefetchCount = settings.PrefetchCount;
        PrefetchSize = settings.PrefetchSize;
        AutoDelete = settings.AutoDelete;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    string GetMac()
    {
        try
        {
            List<string> macs = new List<string>();
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                {
                    continue;
                }
                if (ni.GetPhysicalAddress().ToString() != "")
                {
                    macs.Add(ni.GetPhysicalAddress().ToString());
                }
            }

            //替补mac地址，当找不到以太网mac，则使用第一个mac
            var subs = macs.Count == 0 && interfaces.Length > 0
                ? interfaces[0].GetPhysicalAddress().ToString()
                : string.Empty;
            return macs.Count > 0 ? macs[0] : subs;
        }
        catch (Exception)
        {
        }
        return string.Empty;
    }

    #endregion

    /// <summary>
    /// 消息消费
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        while (true)
        {
            stoppingToken.ThrowIfCancellationRequested();
            await Task.Delay(5000);

            var factory = RabbitMQConfig.FromJson<ConnectionFactory>();
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.BasicQos(prefetchSize: PrefetchSize, prefetchCount: PrefetchCount, global: false); // 限流                    
            channel.ExchangeDeclare(exchange: ExchangeNames.MainExchange, type: ExchangeType.Direct, durable: true, autoDelete: AutoDelete); // 确认exchange

            // 队列
            channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: AutoDelete, arguments: null);
            channel.QueueBind(queue: QueueName, exchange: ExchangeNames.MainExchange, routingKey: RoutingKey); // 路由，监听普通路由                
            channel.QueueBind(queue: QueueName, exchange: ExchangeNames.MainExchange, routingKey: QueueName); // 路由，监听重试路由

            // 消费
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += ConsumerReceived;
            channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            // 定期检查配置更新,如果配置修改了就断开重连
            while (true)
            {
                await Task.Delay(15000);
                if (stoppingToken.IsCancellationRequested)
                {
                    channel.Close();
                    break;
                }
                var cfg = RabbitMQConfig.FromJson<ConnectionFactory>();
                if (factory.HostName != cfg.HostName
                    || factory.Port != cfg.Port
                    || factory.UserName != cfg.UserName
                    || factory.Password != cfg.Password
                    || factory.VirtualHost != cfg.VirtualHost)
                {
                    await Task.Delay(15000); // 断开重连前尽量消费老服务器的数据                            
                    channel.Close();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 消息业务处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    async void ConsumerReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        var context = new MessageContext
        {
            MessageId = eventArgs.BasicProperties?.MessageId,
            QueueName = QueueName,
            RoutingKey = eventArgs.RoutingKey,
        };

        if (string.IsNullOrWhiteSpace(context.MessageId))
        {
            context.MessageId = Guid.NewGuid().ToString("n");
        }

        try
        {
            if (eventArgs.BasicProperties?.Headers != null)
            {
                if (eventArgs.BasicProperties.Headers.TryGetValue(MessageHeaders.Traceparent, out object oTraceId)
                    && oTraceId is byte[] traceValue)
                {
                    context.TraceId = Encoding.UTF8.GetString(traceValue);
                }
                else
                {
                    context.TraceId = $"00-{Guid.NewGuid():n}-{Guid.NewGuid().ToString("n")[16]}-01";
                }
                if (eventArgs.BasicProperties.Headers.TryGetValue(MessageHeaders.FailCount, out object oFailCount))
                {
                    context.FailCount = (int)oFailCount;
                }
                if (eventArgs.BasicProperties.Headers.TryGetValue(MessageHeaders.PublishTime, out object oPublishTime))
                {
                    context.PublishTime = ((long)oPublishTime).ToDateTime();
                }
            }
        }
        catch (Exception err)
        {
            //Logger.WriteLog(Utility.Constants.LogLevel.Error, "MQ消息 上下文解析异常", err);
        }

        var channel = ((EventingBasicConsumer)sender).Model;
        var body = eventArgs.Body.ToArray();
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService(ConsumerType) as IMessageConsumer;
            ExecuteResult result = await consumer.ExecuteMessageAsync(body, context);
            switch (result.ResultCode)
            {
                case ExecuteResultCode.Retry:
                    if (result.Delay > 0)
                    {
                        DeclareDelayQueue(result.Delay, channel, AutoDelete);

                        // 转到延迟重试队列
                        IBasicProperties messageProp = CreateBasicProperties(context, channel);
                        messageProp.Headers[MessageHeaders.Delay] = result.Delay;
                        channel.BasicPublish(exchange: ExchangeNames.DelayExchange, routingKey: QueueName, basicProperties: messageProp, body: body);
                    }
                    else
                    {
                        // 修改header以后 发回原队列
                        IBasicProperties messageProp = CreateBasicProperties(context, channel);
                        channel.BasicPublish(exchange: ExchangeNames.MainExchange, routingKey: QueueName, basicProperties: messageProp, body: body);
                    }
                    break;
                case ExecuteResultCode.Fail:
                    {
                        DeclareQueue(channel, QueueNames.FailedQueue, AutoDelete);

                        // 转到失败队列
                        IBasicProperties messageProp = CreateBasicProperties(context, channel);
                        messageProp.Headers[MessageHeaders.QueueName] = context.QueueName;
                        channel.BasicPublish(exchange: ExchangeNames.MainExchange, routingKey: QueueNames.FailedQueue, basicProperties: messageProp, body: body);
                    }
                    break;
                case ExecuteResultCode.BadBody:
                    {
                        DeclareQueue(channel, QueueNames.WrongQueue, AutoDelete);

                        // 消息内容和格式错误
                        IBasicProperties messageProp = CreateBasicProperties(context, channel);
                        messageProp.Headers[MessageHeaders.QueueName] = context.QueueName;
                        channel.BasicPublish(exchange: ExchangeNames.MainExchange, routingKey: QueueNames.WrongQueue, basicProperties: messageProp, body: body);
                    }
                    break;
                case ExecuteResultCode.Success:
                default:
                    break;
            }
            channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
        }
        catch (Exception err) // mq连接异常？
        {
            //Logger.WriteLog(Utility.Constants.LogLevel.Error, "MQ消息 处理异常", err);
            await Task.Delay(1000);
            channel.BasicReject(eventArgs.DeliveryTag, requeue: true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="channel"></param>
    /// <returns></returns>
    private static IBasicProperties CreateBasicProperties(MessageContext context, IModel channel)
    {
        var messageProp = channel.CreateBasicProperties();
        messageProp.Persistent = true;
        messageProp.MessageId = context.MessageId;
        messageProp.Headers ??= new Dictionary<string, object>();
        messageProp.Headers[MessageHeaders.Traceparent] = context.TraceId;
        messageProp.Headers[MessageHeaders.FailCount] = context.FailCount;
        messageProp.Headers[MessageHeaders.PublishTime] = context.PublishTime.ValueOf();
        return messageProp;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="queueName"></param>
    /// <param name="autoDelete"></param>
    private static void DeclareQueue(IModel channel, string queueName, bool autoDelete)
    {
        if (__lastCheckTimes.TryGetValue(queueName, out var time) && time > DateTime.Now.AddMinutes(-1))
        {
            return;
        }
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: autoDelete);
        channel.QueueBind(queue: queueName, exchange: ExchangeNames.MainExchange, queueName);
        __lastCheckTimes[queueName] = DateTime.Now;
    }

    static ConcurrentDictionary<string, DateTime> __lastCheckTimes = new ConcurrentDictionary<string, DateTime>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="channel"></param>
    /// <param name="autoDelete"></param>
    private static void DeclareDelayQueue(int delay, IModel channel, bool autoDelete)
    {
        if (__lastCheckTimes.TryGetValue(delay.ToString(), out var time) && time > DateTime.Now.AddMinutes(-1))
        {
            return;
        }
        var retryQueueName = QueueNames.RetryQueue + "_" + delay;
        var queueArgs = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = ExchangeNames.MainExchange,
            ["x-message-ttl"] = delay,
        };
        channel.QueueDeclare(queue: retryQueueName, durable: true, exclusive: false, autoDelete: autoDelete, queueArgs);

        var bindHeaders = new Dictionary<string, object>
        {
            [MessageHeaders.Delay] = delay,
        };

        channel.QueueBind(queue: retryQueueName, exchange: ExchangeNames.DelayExchange, "", bindHeaders);
        __lastCheckTimes[delay.ToString()] = DateTime.Now;
    }
}
