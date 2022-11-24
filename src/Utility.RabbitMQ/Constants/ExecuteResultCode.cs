namespace Utility.RabbitMQ.Constants;

/// <summary>
/// 消费结果
/// </summary>
public enum ExecuteResultCode
{
    /// <summary>
    ///  正常返回
    /// </summary>
    Success = 0,

    /// <summary>
    /// 需要重试
    /// </summary>
    Retry = 1,

    /// <summary>
    /// 失败并且已达最大重试次数
    /// </summary>
    Fail = 2,

    /// <summary>
    /// 消息格式异常
    /// </summary>
    BadBody = 3,
}
