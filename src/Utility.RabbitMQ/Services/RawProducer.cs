using Utility.RabbitMQ.Constants;
using Utility.RabbitMQ.Internal;
using Utility.Extensions;
using RabbitMQ.Client;
using System.Text;
using Utility.NetLog;

namespace Utility.RabbitMQ.Services
{
    /// <summary>
    /// 
    /// </summary>
    class RawProducer : IRawProducer
    {
        /// <summary>
        /// ioc
        /// </summary>
        public RawProducer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public string RabbitMQConfig { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task PublishAsync<TMessage>(string appId, string routingKey, TMessage message, PublishOptions options)
        {
            if (string.IsNullOrEmpty(RabbitMQConfig))
            {
                throw new ArgumentNullException($"RabbitMQConfigJson未赋值");
            }
            await Task.Yield();
            var messageId = Guid.NewGuid().ToString("n");
            for (int tryCount = 1; tryCount <= options.MaxRetryCount; tryCount++)
            {
                try
                {
                    using (var channelWrapper = RabbitConnectionPool.GetChannel(appId, RabbitMQConfig))
                    {
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
                            Logger.WriteLog(Utility.Constants.LogLevel.Warning, $"MQ消息 发送成功,routingKey:{routingKey},body:{json}");
                            return;
                        }
                    }
                }
                catch (Exception err)
                {
                    Logger.WriteLog(Utility.Constants.LogLevel.Error, $"MQ消息 发送失败,重试次数{tryCount}", err);
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
