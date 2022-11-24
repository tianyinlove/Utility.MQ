namespace Utility.RabbitMQ
{
    /// <summary>
    /// 推送消息附加参数
    /// </summary>
    public class PublishOptions
    {
        private int maxRetryCount = 3;

        /// <summary>
        /// 日志跟踪id
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// 发送重试次数
        /// </summary>
        public int MaxRetryCount
        {
            get => maxRetryCount;
            set
            {
                if (value > 0)
                {
                    maxRetryCount = value;
                }
            }
        }
    }
}
