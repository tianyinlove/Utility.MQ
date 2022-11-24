using Utility.RabbitMQ.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Utility.RabbitMQ
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var mongo = host.Services.GetRequiredService<IMQMongoDbContext>();
            mongo.Migrate();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "appsettings.json"), optional: true, reloadOnChange: true);
                })
                .ConfigureLogging(builder =>
                {
                    builder.ClearProviders().SetMinimumLevel(LogLevel.Trace);
#if DEBUG
                        builder.AddDebug();
#endif
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
            return builder;
        }
    }
}
