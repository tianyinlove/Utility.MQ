using System;
using System.Collections.Generic;
using System.Text;
using Utility.RabbitMQ.Attributes;

namespace Utility.Core.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RabbitMQ("MQUtilityTest", "memorycache.operate", "CacheRabbitMQConfig")]
    public class DelCacheMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public List<string> Keys { get; set; }
    }
}
