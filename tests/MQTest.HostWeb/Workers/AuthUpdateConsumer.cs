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
    class AuthUpdateConsumer : MessageConsumer<AuthUpdateMessage>
    {
        private readonly ITestService testService;

        /// <summary>
        /// 
        /// </summary>
        public AuthUpdateConsumer(ITestService testService)
        {
            this.testService = testService;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string ConsumerName => "authtest";
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
        public override async Task<bool> ExecuteAsync(AuthUpdateMessage message, MessageContext context)
        {
            if (!string.IsNullOrEmpty(message.UserName))
            {
                await testService.AuthUpdate(message.UserName);
            }
            return await Task.FromResult(true);
        }
    }
}