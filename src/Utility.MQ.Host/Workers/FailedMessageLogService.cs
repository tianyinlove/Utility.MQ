using Utility.RabbitMQ.Constants;
using Utility.RabbitMQ.Documents;
using Utility.RabbitMQ.Repositories;
using Utility.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Utility.RabbitMQ.Workers;

public class EmappFailedMessageLogService : FailedMessageLogService
{
    public EmappFailedMessageLogService(IServiceScopeFactory scopeFactory, IOptionsMonitor<RabbitMQConfig> optionsMonitor) : base(scopeFactory, optionsMonitor)
    {
    }

    public override string AppId => "Emapp";
}

public class ClassicFailedMessageLogService : FailedMessageLogService
{
    public ClassicFailedMessageLogService(IServiceScopeFactory scopeFactory, IOptionsMonitor<RabbitMQConfig> optionsMonitor) : base(scopeFactory, optionsMonitor)
    {
    }

    public override string AppId => "Classic";
}

/// <summary>
/// 记录失败的消息
/// </summary>
public abstract class FailedMessageLogService : BackgroundService
{
    private ILogger<FailedMessageLogService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConfig _config;

    public abstract string AppId { get; }

    public FailedMessageLogService(IServiceScopeFactory scopeFactory, IOptionsMonitor<RabbitMQConfig> optionsMonitor)
    {
        _scopeFactory = scopeFactory;
        _config = optionsMonitor.CurrentValue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        using var scope = _scopeFactory.CreateScope();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<FailedMessageLogService>>();
        var mongo = scope.ServiceProvider.GetRequiredService<IMQMongoDbContext>();
        mongo.Migrate();
        while (true)
        {
            stoppingToken.ThrowIfCancellationRequested();
            await Task.Delay(5000);

            try
            {
                var factory = _config.RabbitMQJson.FromJson<ConnectionFactory>();
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false); // 限流                    
                channel.ExchangeDeclare(exchange: ExchangeNames.MainExchange, type: ExchangeType.Direct, durable: true); // 确认exchange

                // 队列
                channel.QueueDeclare(queue: QueueNames.FailedQueue, durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind(queue: QueueNames.FailedQueue, exchange: ExchangeNames.MainExchange, QueueNames.FailedQueue);

                // 消费
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += ConsumerReceived;
                channel.BasicConsume(queue: QueueNames.FailedQueue, autoAck: false, consumer: consumer);

                // 定期检查配置更新,如果配置修改了就断开重连
                while (true)
                {
                    await Task.Delay(15000);
                    if (stoppingToken.IsCancellationRequested)
                    {
                        channel.Close();
                        break;
                    }
                    var cfg = _config.RabbitMQJson.FromJson<ConnectionFactory>();
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
        if (string.IsNullOrWhiteSpace(eventArgs.BasicProperties?.MessageId))
        {
            return;
        }

        var channel = ((EventingBasicConsumer)sender).Model;

        var doc = new FailedMessageDocument
        {
            Id = eventArgs.BasicProperties.MessageId,
            Body = Encoding.UTF8.GetString(eventArgs.Body.ToArray()),
            AppId = AppId,
            LogTime = DateTime.Now,
        };

        if (eventArgs.BasicProperties.Headers != null)
        {
            if (eventArgs.BasicProperties.Headers.TryGetValue(MessageHeaders.Traceparent, out object oTraceId)
                && oTraceId is byte[] traceValue)
            {
                doc.TraceId = Encoding.UTF8.GetString(traceValue);
            }
            if (eventArgs.BasicProperties.Headers.TryGetValue(MessageHeaders.FailCount, out object oFailCount))
            {
                doc.FailCount = (int)oFailCount;
            }
            if (eventArgs.BasicProperties.Headers.TryGetValue(MessageHeaders.PublishTime, out object oPublishTime))
            {
                doc.PublishTime = ((long)oPublishTime).ToDateTime();
            }
            if (eventArgs.BasicProperties.Headers.TryGetValue(MessageHeaders.QueueName, out object oQueueName)
                && oQueueName is byte[] queueNameValue)
            {
                doc.QueueName = Encoding.UTF8.GetString(queueNameValue);
            }
        }

        using var scope = _scopeFactory.CreateScope();
        var mongo = scope.ServiceProvider.GetRequiredService<IMQMongoDbContext>();
        var filter = Builders<FailedMessageDocument>.Filter.Eq(d => d.Id, doc.Id);
        await mongo.FailedMessageCollection.ReplaceOneAsync(filter, doc, new ReplaceOptions { IsUpsert = true });
        channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
    }
}