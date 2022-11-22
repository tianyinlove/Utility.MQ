using Emapp.Configuration.Model;
using Emapp.Constants;
using Utility.MQ.Constants;
using Utility.MQ.Internal;
using Emapp.Utility.Extensions;
using Emapp.Utility.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace Utility.MQ.Services
{
    class RawProducer : IRawProducer
    {
        private readonly IDictMonitor<string> _configAccessor;
        private readonly ILogger<RawProducer> _logger;

        /// <summary>
        /// ioc
        /// </summary>
        public RawProducer(IDictMonitor<string> configAccessor, ILogger<RawProducer> logger)
        {
            _configAccessor = configAccessor;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task PublishAsync<TMessage>(AppId appId, string routingKey, TMessage message, PublishOptions options)
        {
            await Task.Yield();
            var messageId = Guid.NewGuid().ToString("n");
            for (int tryCount = 1; tryCount <= options.MaxRetryCount; tryCount++)
            {
                var configName = appId.ToString();
                var configJson = _configAccessor.Get(ConfigSections.RabbitMQ)[configName];
                
                try
                {
                    using var channelWrapper = RabbitConnectionPool.GetChannel(appId, configJson);
                    if (channelWrapper?.Channel != null)
                    {
                        var properties = channelWrapper.Channel.CreateBasicProperties();
                        properties.Persistent = true;
                        properties.MessageId = messageId;
                        properties.Headers ??= new Dictionary<string, object>();
                        properties.Headers[RequestKeys.Traceparent] = options.TraceId;
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
