using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Utility.Dependency;
using Utility.RabbitMQ.Services;

namespace Utility.RabbitMQ.Cache
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceLife(ServiceLifeMode.Singleton)]
    internal class MQOperateCacheProducer : IMQOperateCacheProducer
    {
        private readonly IMessageProducer messageProducer;

        /// <summary>
        /// 
        /// </summary>
        public MQOperateCacheProducer(IMessageProducer messageProducer)
        {
            this.messageProducer = messageProducer;
        }

        /// <summary>
        /// 缓存操作
        /// </summary>
        /// <param name="message"></param>
        public async Task Del(MQOperateCacheMessage message)
        {
            if (message != null && message.Keys != null && message.Keys.Length > 0)
            {
                await messageProducer.PublishAsync(message);
            }
        }
    }
}
