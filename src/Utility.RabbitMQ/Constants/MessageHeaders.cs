namespace Utility.RabbitMQ.Constants
{
    /// <summary>
    /// mq消息头
    /// </summary>
    public static class MessageHeaders
    {
        /// <summary>
        /// 失败次数
        /// </summary>
        public const string FailCount = "Products-MQ-FailCount";

        /// <summary>
        /// 原始队列名
        /// </summary>
        public const string QueueName = "Products-MQ-QueueName";

        /// <summary>
        /// 发送时间
        /// </summary>
        public const string PublishTime = "Products-MQ-PublishTime";

        /// <summary>
        /// 延迟消息
        /// </summary>
        public const string Delay = "Products-MQ-Delay";

        /// <summary>
        /// 记录跟踪id，同一个请求过程中所有被调用的webapi接口具有相同的TraceId
        /// </summary>
        public const string Traceparent = "traceparent";
    }
}
