using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility.Core.Common;
using Utility.Core.ISevice;
using Utility.Core.Model;
using Utility.NetLog;
using Utility.RabbitMQ;

namespace MQTest.HostWeb.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class TestController : ControllerBase
    {
        private readonly IMessageProducer messageProducer;
        private readonly ITestService testService;
        private readonly AppSettings config;

        /// <summary>
        /// 
        /// </summary>
        public TestController(IMessageProducer messageProducer, IOptionsMonitor<AppSettings> optionsMonitor, ITestService testService)
        {
            this.messageProducer = messageProducer;
            this.testService = testService;
            config = optionsMonitor.CurrentValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<string>> GetAuth()
        {
            return await testService.GetAuth();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task Del()
        {
            await messageProducer.PublishAsync(new DelCacheMessage { Keys = new List<string> { "appcache:auth:uuidauth" } });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task UpdateAuth(string name = "测试用户")
        {
            Logger.WriteLog(Utility.Constants.LogLevel.Info, "测试MQ消息");
            messageProducer.RabbitMQConfig = config.RabbitMQConfig;
            await messageProducer.PublishAsync(new AuthUpdateMessage { UserName = name });
        }
    }
}
