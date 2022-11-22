namespace Utility.MQ.Constants;

/// <summary>
/// 消息处理结果
/// </summary>
public class ExecuteResult
{
    /// <summary>
    /// 
    /// </summary>
    public ExecuteResultCode ResultCode { get; set; }

    /// <summary>
    /// 重试延迟，毫秒
    /// </summary>
    public int Delay { get; internal set; }
    public string ErrorMessage { get; internal set; }
}
