using System;
using System.Collections.Generic;
using System.Text;
using Utility.RabbitMQ.Attributes;

namespace Utility.RabbitMQ.Cache
{
    /// <summary>
    /// 缓存操作消息
    /// </summary>
    [RabbitMQ("MQCahceOperate", "cache.operate")]
    public class MQOperateCacheMessage
    {
        /// <summary>
        /// Redis服务器名(如果为空则只操作内存)
        /// </summary>
        public string RedisName { get; set; }
        /// <summary>
        /// 需要操作的key
        /// </summary>
        public string[] Keys { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string[] Fields { get; set; }
    }
}
