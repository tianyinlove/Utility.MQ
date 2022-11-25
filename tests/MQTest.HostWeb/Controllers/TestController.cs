using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility.NetLog;
using Utility.RabbitMQ;
using Utility.RabbitMQ.Cache;

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
        private readonly AppSettings config;

        /// <summary>
        /// 
        /// </summary>
        public TestController(IMessageProducer messageProducer, IOptionsMonitor<AppSettings> optionsMonitor)
        {
            this.messageProducer = messageProducer;
            config = optionsMonitor.CurrentValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task Push()
        {
            Logger.WriteLog(Utility.Constants.LogLevel.Info, "测试MQ消息");
            //messageProducer.RabbitMQConfig = config.RabbitMQConfig;
            await messageProducer.PublishAsync(new MQOperateCacheMessage { Keys = new string[] { "appcache:auth:uuidauth" } });
        }
    }
}
