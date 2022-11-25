namespace Utility.RabbitMQ.Services
{
    /// <summary>
    /// 
    /// </summary>
    interface IRawProducer
    {
        /// <summary>
        /// 
        /// </summary>
        string RabbitMQConfig { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="appId"></param>
        /// <param name="routingKey"></param>
        /// <param name="message"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task PublishAsync<TMessage>(string appId, string routingKey, TMessage message, PublishOptions options);
    }
}
