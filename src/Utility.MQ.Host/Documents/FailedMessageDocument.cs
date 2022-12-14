using Emapp.Constants;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Utility.RabbitMQ.Documents;

[BsonIgnoreExtraElements]
public class FailedMessageDocument
{
    /// <summary>
    /// _id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 消息appid
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    /// 消费队列名称
    /// </summary>
    public string QueueName { get; set; }

    /// <summary>
    /// 消息发布时间
    /// </summary>
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime PublishTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime LogTime { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// 跟踪id
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    /// 失败次数
    /// </summary>
    public int FailCount { get; set; }
}
