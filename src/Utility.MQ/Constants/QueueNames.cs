namespace Utility.MQ.Constants;

/// <summary>
/// 
/// </summary>
public static class QueueNames
{
    /// <summary>
    /// 重试队列
    /// </summary>
    public const string RetryQueue = "emapp_delay";

    /// <summary>
    /// 失败的队列
    /// </summary>
    public const string FailedQueue = "emapp_failed";

    /// <summary>
    /// 失败的队列
    /// </summary>
    public const string WrongQueue = "emapp_wrong";
}
