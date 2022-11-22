namespace Utility.MQ.Constants
{
    /// <summary>
    /// mq消息头
    /// </summary>
    public static class MessageHeaders
    {
        /// <summary>
        /// 失败次数
        /// </summary>
        public const string FailCount = "Emapp-FailCount";

        /// <summary>
        /// 原始队列名
        /// </summary>
        public const string QueueName = "Emapp-QueueName";

        /// <summary>
        /// 发送时间
        /// </summary>
        public const string PublishTime = "Emapp-PublishTime";

        /// <summary>
        /// 延迟消息
        /// </summary>
        public const string Delay = "Emapp-Delay";
    }
}
