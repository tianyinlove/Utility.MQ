using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
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

        /// <summary>
        /// 
        /// </summary>
        public TestController(IMessageProducer messageProducer)
        {
            this.messageProducer = messageProducer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task Push()
        {
            Logger.WriteLog(Utility.Constants.LogLevel.Info, "测试MQ消息");
            await messageProducer.PublishAsync(new MQOperateCacheMessage { Keys = new string[] { "appcache:auth:uuidauth" } });
        }
    }
}
