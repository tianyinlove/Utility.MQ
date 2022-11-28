using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utility.Core.Common;
using Utility.Core.IData;
using Utility.Redis;

namespace Utility.Core.Data
{
    /// <summary>
    /// 
    /// </summary>
    internal class TestData : ITestData
    {
        private readonly AppSettings config;

        /// <summary>
        /// 
        /// </summary>
        public TestData(IOptionsMonitor<AppSettings> optionsMonitor)
        {
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
        /// <returns></returns>
        public async Task<List<string>> GetAuth()
        {
            var cacheKey = "appcache:auth:uuidauth:data";
            var result = redisClient.Get<List<string>>(cacheKey);
            if (result == null)
            {
                result = new List<string> { "123456", "234567" };
                redisClient.Set(cacheKey, result, TimeSpan.FromDays(30));
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<bool> SetAuth(string name)
        {
            var result = false;
            var data = await GetAuth() ?? new List<string>();
            if (!string.IsNullOrEmpty(name))
            {
                data.Add(name);
                var cacheKey = "appcache:auth:uuidauth:data";
                result = redisClient.Set(cacheKey, data, TimeSpan.FromDays(30));
            }
            return result;
        }
    }
}
