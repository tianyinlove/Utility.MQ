using Emapp.Constants;

namespace Utility.MQ.Services
{
    interface IRawProducer
    {
        Task PublishAsync<TMessage>(AppId appId, string routingKey, TMessage message, PublishOptions options);
    }
}
