using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using Utility.RabbitMQ;
using Utility.RabbitMQ.Cache;

namespace MQTest.HostWeb.Workers
{

    /// <summary>
    /// 缓存操作消费者
    /// </summary>
    class MQOperateCacheConsumer : MessageConsumer<MQOperateCacheMessage>
    {
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// 
        /// </summary>
        public MQOperateCacheConsumer(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string ConsumerName => "cachetest";
        /// <summary>
        /// 
        /// </summary>
        public override string ConsumerAppId => "MQTest";

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