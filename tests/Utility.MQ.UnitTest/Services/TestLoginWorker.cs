using Utility.RabbitMQ.UnitTest.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System;

namespace Utility.RabbitMQ.UnitTest.Services
{
    /// <summary>
    /// 测试消费者
    /// </summary>
    public class TestLoginWorker : BaseTest
    {
        /// <summary>
        /// 测试消息发布
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Test()
        {
            var host = await StartHostAsync();
            var tasks = new List<Task>();
            var agent = host.Services.GetRequiredService<IMessageProducer>();

            await agent.PublishAsync<LoginMessage>(new LoginMessage { UserName = "中文" });

            await Task.Delay(60 * 60 * 1000);
        }
    }
}
