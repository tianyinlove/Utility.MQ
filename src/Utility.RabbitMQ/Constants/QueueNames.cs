namespace Utility.RabbitMQ.Constants;

/// <summary>
/// 
/// </summary>
public static class QueueNames
{
    /// <summary>
    /// 重试队列
    /// </summary>
    public const string RetryQueue = "cachemq_delay";

    /// <summary>
    /// 失败的队列
    /// </summary>
    public const string FailedQueue = "cachemq_failed";

    /// <summary>
    /// 失败的队列
    /// </summary>
    public const string WrongQueue = "cachemq_wrong";
}
