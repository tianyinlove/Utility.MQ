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
        public const string MainExchange = "products_mq_main";

        /// <summary>
        /// 延迟交换机 dlx
        /// </summary>
        public const string DelayExchange = "products_mq_delay";
    }
}
