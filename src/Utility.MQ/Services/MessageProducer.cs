using Utility.MQ.Services;
using Utility.Dependency;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Utility.MQ.Attributes;

namespace Utility.MQ
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

            options ??= new();
            if (string.IsNullOrWhiteSpace(options.TraceId))
            {
                options.TraceId = $"00-{Guid.NewGuid():n}-{Guid.NewGuid().ToString("n")[..16]}-01";
            }

            using var scope = _scopeFactory.CreateScope();
            var rawAgent = scope.ServiceProvider.GetRequiredService<IRawProducer>();
            await rawAgent.PublishAsync<TMessage>(appId, routingKey, message, options);
        }
    }
}
