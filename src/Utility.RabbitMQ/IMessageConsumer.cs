using Utility.RabbitMQ.Constants;

namespace Utility.RabbitMQ
{
    /// <summary>
    /// MQ设置
    /// </summary>
    public interface IMessageConsumer
    {
        /// <summary>
        /// MQ配置，Json格式
        /// </summary>
        string RabbitMQConfig { get; }

        /// <summary>
        /// 队列重试次数
        /// </summary>
        int MaxRetry { get; }

        /// <summary>
        /// 首次收到消息的原地重试次数
        /// </summary>
        int MaxLocalRetry { get; }

        /// <summary>
        /// 队列重试延迟
        /// </summary>
        /// <param name="failCount"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        int GetRetryDelay(int failCount, MessageContext context);

        /// <summary>
        /// 消费模块名
        /// </summary>
        string ConsumerName { get; }

        /// <summary>
        /// 消费者应用id
        /// </summary>
        string ConsumerAppId { get; }

        /// <summary>
        /// 
        /// </summary>
        ushort PrefetchCount { get; }

        /// <summary>
        /// 
        /// </summary>
        ushort PrefetchSize { get; }

        /// <summary>
        /// 处理mq消息
        /// </summary>
        /// <param name="body"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<ExecuteResult> ExecuteMessageAsync(byte[] body, MessageContext context);
    }
}
