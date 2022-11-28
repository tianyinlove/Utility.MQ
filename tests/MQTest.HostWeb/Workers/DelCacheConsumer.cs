using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Utility.Core.ISevice;
using Utility.Core.Model;
using Utility.RabbitMQ;

namespace MQTest.HostWeb.Workers
{

    /// <summary>
    /// 缓存操作消费者
    /// </summary>
    class DelCacheConsumer : MessageConsumer<DelCacheMessage>
    {
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// 
        /// </summary>
        public DelCacheConsumer(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string ConsumerName => "memorytest";
        /// <summary>
        /// 
        /// </summary>
        public override string ConsumerAppId => "MQTest";
        /// <summary>
        /// 
        /// </summary>
        public override bool AutoDelete => true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<bool> ExecuteAsync(DelCacheMessage message, MessageContext context)
        {
            if (message.Keys != null)
            {
                foreach (var key in message.Keys)
                {
                    memoryCache.Remove(key);
                }
            }
            return await Task.FromResult(true);
        }
    }
}