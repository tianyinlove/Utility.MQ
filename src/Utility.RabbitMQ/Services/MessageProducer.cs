using Utility.RabbitMQ.Services;
using Utility.Dependency;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Utility.RabbitMQ.Attributes;
using Microsoft.Extensions.Configuration;

namespace Utility.RabbitMQ
{
    /// <summary>
    /// 消息推送代理
    /// </summary>
    [ServiceLife(ServiceLifeMode.Singleton)]
    class MessageProducer : IMessageProducer
    {
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// ioc
        /// </summary>
        public MessageProducer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        public string RabbitMQConfig { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task PublishAsync<TMessage>(TMessage message, PublishOptions options = null)
        {
            if (typeof(TMessage).GetCustomAttribute<RabbitMQAttribute>() is not RabbitMQAttribute keyAttribute
                || string.IsNullOrWhiteSpace(keyAttribute.RouteKey))
            {
                throw new ArgumentNullException($"无法识别RoutingKey，请通过{nameof(RabbitMQAttribute)}标注");
            }
            var routingKey = keyAttribute.RouteKey;
            string appId = keyAttribute.AppId;

            using (var scope = _scopeFactory.CreateScope())
            {
                if (string.IsNullOrEmpty(RabbitMQConfig))
                {
                    if (string.IsNullOrEmpty(keyAttribute.ConfigName))
                    {
                        throw new ArgumentNullException($"无法识别MQ名，请通过{nameof(RabbitMQAttribute)}标注或赋值RabbitMQConfig");
                    }
                    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    RabbitMQConfig = configuration.GetSection(keyAttribute.ConfigName).Value;
                    if (string.IsNullOrEmpty(RabbitMQConfig))
                    {
                        throw new ArgumentNullException($"{keyAttribute.ConfigName}未配置，请配置appsettings.json或赋值RabbitMQConfig");
                    }
                }
                options ??= new PublishOptions();
                if (string.IsNullOrWhiteSpace(options.TraceId))
                {
                    options.TraceId = $"00-{Guid.NewGuid():n}-{Guid.NewGuid().ToString("n")[..16]}-01";
                }

                var rawAgent = scope.ServiceProvider.GetRequiredService<IRawProducer>();
                rawAgent.RabbitMQConfig = RabbitMQConfig;
                await rawAgent.PublishAsync<TMessage>(appId, routingKey, message, options);
            }
        }
    }
}
