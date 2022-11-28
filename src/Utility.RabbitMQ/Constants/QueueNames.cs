namespace Utility.RabbitMQ.Constants;

/// <summary>
/// 
/// </summary>
public static class QueueNames
{
    /// <summary>
    /// 重试队列
    /// </summary>
    public const string RetryQueue = "products_mq_delay";

    /// <summary>
    /// 失败的队列
    /// </summary>
    public const string FailedQueue = "products_mq_failed";

    /// <summary>
    /// 失败的队列
    /// </summary>
    public const string WrongQueue = "products_mq_wrong";
}
