namespace Utility.MQ
{
    /// <summary>
    /// 消息上下文
    /// </summary>
    public class MessageContext
    {
        /// <summary>
        /// 日志跟踪
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// 消息id
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// 消息路由
        /// </summary>
        public string RoutingKey { get; set; }

        /// <summary>
        /// 失败次数
        /// </summary>
        public int FailCount { get; set; }

        /// <summary>
        /// 本地失败次数
        /// </summary>
        public int LocalFailCount { get; set; }

        /// <summary>
        /// 消息发布时间
        /// </summary>
        public DateTime PublishTime { get; set; }
    }
}
