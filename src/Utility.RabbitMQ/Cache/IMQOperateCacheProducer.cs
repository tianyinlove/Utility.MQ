using System;
using System.Collections.Generic;
using System.Text;

namespace Utility.RabbitMQ.Cache
{
    /// <summary>
    /// MQ操作缓存
    /// </summary>
    public interface IMQOperateCacheProducer
    {
        /// <summary>
        /// 缓存操作
        /// </summary>
        /// <param name="message"></param>
        Task Del(MQOperateCacheMessage message);
    }
}
