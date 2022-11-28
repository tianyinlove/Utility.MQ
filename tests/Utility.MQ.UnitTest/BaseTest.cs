using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using Utility.Extensions;
using Utility.Core.Common;

namespace Utility.RabbitMQ.UnitTest
{
    /// <summary>
    /// 测试公共方法
    /// </summary>
    public class BaseTest
    {
        /// <summary>
        /// 配置信息
        /// </summary>
        public IConfiguration Configuration { get; private set; }

        private void ConfigServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration); //自定义配置配置
            services.AddMQService();
        }


        IHost _host;
        /// <summary>
        /// 启动generic host,可以测试HostedService相关功能
        /// </summary>
        protected async Task<IHost> StartHostAsync()
        {
            _host = new HostBuilder()
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    configBuilder
                        .SetBasePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"))
                        .AddJsonFile("appsettings.json", optional: true);
                })
                .ConfigureServices((context, services) =>
                {
                    Configuration = new ConfigurationBuilder()
                                         .SetBasePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"))
                                         .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                         .Build();
                    ConfigServices(services);
                })
                .Build();
            await _host.StartAsync();
            return _host;
        }

        /// <summary>
        /// 关闭host
        /// </summary>
        /// <returns></returns>
        protected async Task StopHostAsync()
        {
            try
            {
                if (_host != null)
                {
                    await _host.StopAsync();
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <summary>
        /// ioc容器
        /// </summary>
        protected IServiceProvider CreateProvider()
        {
            Configuration = new ConfigurationBuilder()
                                 .SetBasePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"))
                                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                 .Build();
            var services = new ServiceCollection();
            ConfigServices(services);
            return services.BuildServiceProvider();
        }
    }
}
