using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utility.Core.Common;
using Utility.Core.Data;
using Utility.Core.IData;
using Utility.Core.ISevice;
using Utility.Core.Model;
using Utility.RabbitMQ;
using Utility.Redis;

namespace Utility.Core.Service
{
    /// <summary>
    /// 
    /// </summary>
    internal class TestService : ITestService
    {
        private readonly ITestData testData;
        private readonly IMemoryCache memoryCache;
        private readonly IMessageProducer messageProducer;
        private readonly AppSettings config;

        /// <summary>
        /// 
        /// </summary>
        public TestService(ITestData testData, IMemoryCache memoryCache,
            IOptionsMonitor<AppSettings> optionsMonitor,
            IMessageProducer messageProducer)
        {
            this.testData = testData;
            this.memoryCache = memoryCache;
            this.messageProducer = messageProducer;
            config = optionsMonitor.CurrentValue;
        }

        /// <summary>
        /// 
        /// </summary>
        RedisClient redisClient
        {
            get
            {
                return RedisClient.GetInstance(config.emappservice);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isUpdate"></param>
        /// <returns></returns>
        public async Task<List<string>> GetAuth(bool isUpdate = false)
        {
            var cacheKey = "appcache:auth:uuidauth";
            var result = memoryCache.Get<List<string>>(cacheKey);
            if (result == null || isUpdate)
            {
                result = redisClient.Get<List<string>>(cacheKey);
                if (result == null || isUpdate)
                {
                    result = await testData.GetAuth();
                    redisClient.Set(cacheKey, result, TimeSpan.FromDays(30));
                }
                memoryCache.Set(cacheKey, result, TimeSpan.FromHours(1));
            }
            if (isUpdate)
            {
                //清理内存

            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<bool> AuthUpdate(string userName)
        {
            //更新数据
            await testData.SetAuth(userName);

            return await Task.FromResult(true);
        }
    }
}
