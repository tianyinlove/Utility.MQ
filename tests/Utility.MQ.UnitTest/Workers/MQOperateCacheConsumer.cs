using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utility.Core.Common;
using Utility.RabbitMQ.Cache;

namespace Utility.RabbitMQ.UnitTest.Workers
{
    /// <summary>
    /// 缓存操作消费者
    /// </summary>
    class MQOperateCacheConsumer : MessageConsumer<MQOperateCacheMessage>
    {
        private readonly IMemoryCache memoryCache;
        private readonly AppSettings config;

        /// <summary>
        /// 
        /// </summary>
        public MQOperateCacheConsumer(IMemoryCache memoryCache, IOptionsMonitor<AppSettings> optionsMonitor)
        {
            this.memoryCache = memoryCache;
            config = optionsMonitor.CurrentValue;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string ConsumerName => "emtest4";
        /// <summary>
        /// 
        /// </summary>
        public override string ConsumerAppId => "Emapp";

        /// <summary>
        /// 
        /// </summary>
        public override string RabbitMQConfig => config.CacheRabbitMQConfig;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<bool> ExecuteAsync(MQOperateCacheMessage message, MessageContext context)
        {
            if (string.IsNullOrEmpty(message.RedisName))
            {
                MemoryOperate(message);
            }
            else
            {
                RedisOperate(message);
            }
            return await Task.FromResult(true);
        }

        /// <summary>
        /// 内存操作
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        void MemoryOperate(MQOperateCacheMessage message)
        {
            foreach (var key in message.Keys)
            {
                memoryCache.Remove(message.Keys);
            }
        }

        /// <summary>
        /// redis操作
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        void RedisOperate(MQOperateCacheMessage message)
        {

        }
    }
}
