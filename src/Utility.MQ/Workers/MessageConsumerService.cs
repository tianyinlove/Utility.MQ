using Emapp.Attributes;
using Emapp.Configuration.Model;
using Emapp.Constants;
using Utility.MQ.Constants;
using Emapp.Utility.Extensions;
using Emapp.Utility.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace Utility.MQ;

/// <summary>
/// 
/// </summary>
internal class MessageConsumerService<TComsumer> : BackgroundService
{
    #region 变量和ioc

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MessageConsumerService<TComsumer>> _logger;
    private readonly IDictMonitor<string> _configAccessor;

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
    private AppId ProducerAppId { get; set; }

    /// <summary>
    /// 消息路由
    /// </summary>
    private string RoutingKey { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string QueueName { get; set; }

    /// <summary>
    /// ioc
    /// </summary>
    public MessageConsumerService(IServiceScopeFactory scopeFactory, ILogger<MessageConsumerService<TComsumer>> logger, IDictMonitor<string> configAccessor)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configAccessor = configAccessor;
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

        if (MessageType.GetCustomAttribute<EmappMQAttribute>() is EmappMQAttribute keyAttribute
            && !string.IsNullOrWhiteSpace(keyAttribute.RouteKey))
        {
            RoutingKey = keyAttribute.RouteKey;
            ProducerAppId = keyAttribute.AppId;
        }
        else
        {
            throw new ArgumentNullException("未标记EmappMQAttribute并设置routingkey");
        }
        using var scope = _scopeFactory.CreateScope();
        var settings = scope.ServiceProvider.GetRequiredService(ConsumerType) as IMessageConsumer;

        QueueName = settings.ConsumerAppId == ProducerAppId
            ? $"{settings.ConsumerName}.{RoutingKey}"
            : $"{settings.ConsumerAppId.ToString().ToLower()}.{settings.ConsumerName}.{RoutingKey}";
        PrefetchCount = settings.PrefetchCount;
        PrefetchSize = settings.PrefetchSize;
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

            try
            {
                var factory = _configAccessor.Get(ConfigSections.RabbitMQ)[ProducerAppId.ToString()].FromJson<ConnectionFactory>();
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.BasicQos(prefetchSize: PrefetchSize, prefetchCount: PrefetchCount, global: false); // 限流                    
                channel.ExchangeDeclare(exchange: ExchangeNames.MainExchange, type: ExchangeType.Direct, durable: true); // 确认exchange

                // 队列
                channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
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
                    var cfg = _configAccessor.Get(ConfigSections.RabbitMQ)[ProducerAppId.ToString()].FromJson<ConnectionFactory>();
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
            catch (Exception err)
            {
                _logger.LogWarning(err, "处理mq消息异常");
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
                if (eventArgs.BasicProperties.Headers.TryGetValue(RequestKeys.Traceparent, out object oTraceId)
                    && oTraceId is byte[] traceValue)
                {
                    context.TraceId = Encoding.UTF8.GetString(traceValue);
                }
                else
                {
                    context.TraceId = $"00-{Guid.NewGuid():n}-{Guid.NewGuid().ToString("n")[..16]}-01";
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
            _logger.LogWarning(err, "MQ消息 上下文解析异常");
        }

        var channel = ((EventingBasicConsumer)sender).Model;
        var body = eventArgs.Body.ToArray();
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService(ConsumerType) as IMessageConsumer;
            ExecuteResult result = await consumer.ExecuteMessageAsync(body, context, _logger);
            switch (result.ResultCode)
            {
                case ExecuteResultCode.Retry:
                    if (result.Delay > 0)
                    {
                        DeclareDelayQueue(result.Delay, channel);

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
                        DeclareQueue(channel, QueueNames.FailedQueue);

                        // 转到失败队列
                        IBasicProperties messageProp = CreateBasicProperties(context, channel);
                        messageProp.Headers[MessageHeaders.QueueName] = context.QueueName;
                        channel.BasicPublish(exchange: ExchangeNames.MainExchange, routingKey: QueueNames.FailedQueue, basicProperties: messageProp, body: body);
                    }
                    break;
                case ExecuteResultCode.BadBody:
                    {
                        DeclareQueue(channel, QueueNames.WrongQueue);

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
            _logger.LogWarning(err, "MQ消息 处理异常");
            await Task.Delay(1000);
            channel.BasicReject(eventArgs.DeliveryTag, requeue: true);
        }
    }

    private static IBasicProperties CreateBasicProperties(MessageContext context, IModel channel)
    {
        var messageProp = channel.CreateBasicProperties();
        messageProp.Persistent = true;
        messageProp.MessageId = context.MessageId;
        messageProp.Headers ??= new Dictionary<string, object>();
        messageProp.Headers[RequestKeys.Traceparent] = context.TraceId;
        messageProp.Headers[MessageHeaders.FailCount] = context.FailCount;
        messageProp.Headers[MessageHeaders.PublishTime] = context.PublishTime.ValueOf();
        return messageProp;
    }

    private static void DeclareQueue(IModel channel, string queueName)
    {
        if (__lastCheckTimes.TryGetValue(queueName, out var time) && time > DateTime.Now.AddMinutes(-1))
        {
            return;
        }
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue: queueName, exchange: ExchangeNames.MainExchange, queueName);
        __lastCheckTimes[queueName] = DateTime.Now;
    }

    static ConcurrentDictionary<string, DateTime> __lastCheckTimes = new ConcurrentDictionary<string, DateTime>();
    private static void DeclareDelayQueue(int delay, IModel channel)
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
        channel.QueueDeclare(queue: retryQueueName, durable: true, exclusive: false, autoDelete: false, queueArgs);

        var bindHeaders = new Dictionary<string, object>
        {
            [MessageHeaders.Delay] = delay,
        };

        channel.QueueBind(queue: retryQueueName, exchange: ExchangeNames.DelayExchange, "", bindHeaders);
        __lastCheckTimes[delay.ToString()] = DateTime.Now;
    }
}
