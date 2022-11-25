using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utility.RabbitMQ.Cache;

namespace Utility.Core.ISevice
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
