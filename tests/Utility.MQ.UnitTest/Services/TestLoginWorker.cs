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

            for (int i = 0; i < 1000; i++)
            {
                await agent.PublishAsync<LoginMessage>(new LoginMessage { UserName = "中文 啊实打实😫o((>ω< ))o等等❤😎🌹😃🐱‍👤💖😢😢🤞✔🎂😎✌✌👏🤦‍♂️👌👌😂😁₩㎡￡₠㎝( •̀ ω •́ )✧( •̀ ω •́ )✧<(￣︶￣)↗[GO!](✿◡‿◡)o(*^＠^*)oo(*^＠^*)o(～￣▽￣)～O(∩_∩)O（￣︶￣）↗　aaφ(*￣0￣)ヾ(≧▽≦*)oヾ(≧▽≦*)o😍🐱‍💻🐱‍🐉🤢🎁🤷‍♂️😏🤗😎🙂🥙🌯🥞🥞💤💞☯🕉💗💗💗大大阿萨大大发撒乱码%*&#@！——#&……￥*（@！@！#……@——#！@……#……！" });
            }

            await Task.Delay(5000000);

        }
    }
}
