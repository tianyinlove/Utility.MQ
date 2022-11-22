using Utility.MQ.Constants;
using Utility.MQ.Internal;
using Utility.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Options;

namespace Utility.MQ.Services
{
    /// <summary>
    /// 
    /// </summary>
    class RawProducer : IRawProducer
    {
        private readonly RabbitMQConfig _config;
        private readonly ILogger<RawProducer> _logger;

        /// <summary>
        /// ioc
        /// </summary>
        public RawProducer(IOptionsMonitor<RabbitMQConfig> optionsMonitor, ILogger<RawProducer> logger)
        {
            _config = optionsMonitor.CurrentValue;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task PublishAsync<TMessage>(string appId, string routingKey, TMessage message, PublishOptions options)
        {
            await Task.Yield();
            var messageId = Guid.NewGuid().ToString("n");
            for (int tryCount = 1; tryCount <= options.MaxRetryCount; tryCount++)
            {
                try
                {
                    using var channelWrapper = RabbitConnectionPool.GetChannel(appId, _config.RabbitMQJson);
                    if (channelWrapper?.Channel != null)
                    {
                        var properties = channelWrapper.Channel.CreateBasicProperties();
                        properties.Persistent = true;
                        properties.MessageId = messageId;
                        properties.Headers ??= new Dictionary<string, object>();
                        properties.Headers[MessageHeaders.Traceparent] = options.TraceId;
                        properties.Headers[MessageHeaders.PublishTime] = DateTime.Now.ValueOf();
                        var json = message.ToApiJson();
                        var body = Encoding.UTF8.GetBytes(json);

                        channelWrapper.Channel.BasicPublish(exchange: ExchangeNames.MainExchange, routingKey: routingKey, basicProperties: properties, body: body);
                        _logger.LogInformation($"MQ消息 发送成功,routingKey:{routingKey},body:{json}");
                        return;
                    }
                }
                catch (Exception err)
                {
                    _logger.LogError(err, $"MQ消息 发送失败,重试次数{tryCount}");
                }
                if (tryCount == options.MaxRetryCount)
                {
                    throw new Exception("MQ消息发送失败");
                }
                await Task.Delay(100 * tryCount);
            }
        }
    }
}
