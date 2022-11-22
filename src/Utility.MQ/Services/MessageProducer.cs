using Emapp.Attributes;
using Emapp.Constants;
using Utility.MQ.Services;
using Emapp.Utility.Dependency;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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

        public async Task PublishAsync<TMessage>(TMessage message, PublishOptions options = null)
        {
            if (typeof(TMessage).GetCustomAttribute<EmappMQAttribute>() is not EmappMQAttribute keyAttribute
                || string.IsNullOrWhiteSpace(keyAttribute.RouteKey))
            {
                throw new ArgumentNullException($"无法识别RoutingKey，请通过{nameof(EmappMQAttribute)}标注");
            }
            var routingKey = keyAttribute.RouteKey;
            AppId appId = keyAttribute.AppId;

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
