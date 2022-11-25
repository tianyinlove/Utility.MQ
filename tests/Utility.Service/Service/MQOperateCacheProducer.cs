using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utility.Core.Common;
using Utility.Core.ISevice;
using Utility.Dependency;
using Utility.RabbitMQ;
using Utility.RabbitMQ.Cache;
using Utility.RabbitMQ.Services;

namespace Utility.Core.Service
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceLife(ServiceLifeMode.Singleton)]
    internal class MQOperateCacheProducer : IMQOperateCacheProducer
    {
        private readonly IMessageProducer messageProducer;
        private readonly AppSettings config;

        /// <summary>
        /// 
        /// </summary>
        public MQOperateCacheProducer(IMessageProducer messageProducer, IOptionsMonitor<AppSettings> optionsMonitor)
        {
            this.messageProducer = messageProducer;
            config = optionsMonitor.CurrentValue;
        }

        /// <summary>
        /// 缓存操作
        /// </summary>
        /// <param name="message"></param>
        public async Task Del(MQOperateCacheMessage message)
        {
            if (message != null && message.Keys != null && message.Keys.Length > 0)
            {
                messageProducer.RabbitMQConfig = config.CacheRabbitMQConfig;
                await messageProducer.PublishAsync(message);
            }
        }
    }
}
