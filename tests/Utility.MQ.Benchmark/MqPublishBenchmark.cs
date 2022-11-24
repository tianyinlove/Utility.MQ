using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Utility.Extensions;
using Utility.RabbitMQ.Cache;

namespace Utility.RabbitMQ.Benchmark
{
    [SimpleJob(targetCount: 10, warmupCount: 1, invocationCount: 1000)]
    public class MqPublishBenchmark
    {
        public static ServiceProvider _serviceProvider { get; set; }
        const string Message = "中文 啊实打实😫o((>ω< ))o等等❤😎🌹😃🐱‍👤💖😢😢🤞✔🎂😎✌✌👏🤦‍♂️👌👌😂😁₩㎡￡₠㎝( •̀ ω •́ )✧( •̀ ω •́ )✧<(￣︶￣)↗[GO!](✿◡‿◡)o(*^＠^*)oo(*^＠^*)o(～￣▽￣)～O(∩_∩)O（￣︶￣）↗　aaφ(*￣0￣)ヾ(≧▽≦*)oヾ(≧▽≦*)o😍🐱‍💻🐱‍🐉🤢🎁🤷‍♂️😏🤗😎🙂🥙🌯🥞🥞💤💞☯🕉💗💗💗大大阿萨大大发撒乱码%*&#@！——#&……￥*（@！@！#……@——#！@……#……！";

        [GlobalSetup]
        public static void Setup()
        {
            //配置
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"))
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            services.Configure<RabbitMQConfig>(configuration); //自定义配置配置
            services.AddMQOperateCache();
            services.AddMemoryCache();
            services.AddHttpClient();
            _serviceProvider = services.BuildServiceProvider();
        }

        [Benchmark]
        public async Task SingleDelCache()
        {
            var agent = _serviceProvider.GetService<IMessageProducer>();
            await agent.PublishAsync(new MQOperateCacheMessage { Keys = new string[] { "appcache:auth:uuidauth" } });
        }

        [Benchmark]
        public async Task MultiPublish()
        {
            var agent = _serviceProvider.GetService<IMessageProducer>();
            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < 20; i++)
                    {
                        await agent.PublishAsync<LoginData>(new LoginData { UserName = Message });
                    }
                }));
            }
            await Task.WhenAll(tasks);
        }

        [Benchmark]
        public async Task SinglePublish()
        {
            var agent = _serviceProvider.GetService<IMessageProducer>();
            await agent.PublishAsync<LoginData>(new LoginData { UserName = Message });
        }
    }
}