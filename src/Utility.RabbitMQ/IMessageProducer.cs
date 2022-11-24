namespace Utility.RabbitMQ
{
    /// <summary>
    /// 消息推送代理
    /// </summary>
    public interface IMessageProducer
    {
        /// <summary>
        /// 发布消息,类型T必须标记EmappMQAttribute
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message">消息内容</param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task PublishAsync<TMessage>(TMessage message, PublishOptions options = null);
    }
}
