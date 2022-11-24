namespace Utility.RabbitMQ.Constants
{
    /// <summary>
    /// 交换机
    /// </summary>
    public static class ExchangeNames
    {
        /// <summary>
        /// 主交换机
        /// </summary>
        public const string MainExchange = "cachemq_main";

        /// <summary>
        /// 延迟交换机 dlx
        /// </summary>
        public const string DelayExchange = "cachemq_delay";
    }
}
