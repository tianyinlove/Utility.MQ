using Emapp.Attributes;
using Emapp.Configuration;
using Emapp.Configuration.Model;
using Utility.MQ;
using Utility.MQ.Services;
using Emapp.Utility.Dependency;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Emapp.Extensions
{
    /// <summary>
    /// MQ服务客户端扩展方法
    /// </summary>
    public static class MQClientExtension
    {
        /// <summary>
        /// 添加MQ服务客户端
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hostConsumers">是否自动查询MQConsumerService并注册HostedService</param>
        /// <returns></returns>
        public static IServiceCollection AddEmappMQ(this IServiceCollection services, bool hostConsumers = true)
        {
            return services.AddEmappMQProducer().AddEmappMQConsumer(hostConsumers);
        }

        /// <summary>
        /// 添加MQ服务客户端
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEmappMQProducer(this IServiceCollection services)
        {
            services.AddEmappRequire(typeof(IDictMonitor<string>));
            services.TryAddSingleton<IMessageProducer, MessageProducer>();
            services.TryAddTransient<IRawProducer, RawProducer>();
            return services;
        }
        /// <summary>
        /// 添加MQ服务客户端
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hostConsumers">是否自动查询MQConsumerService并注册HostedService</param>
        /// <returns></returns>
        public static IServiceCollection AddEmappMQConsumer(this IServiceCollection services, bool hostConsumers = true)
        {
            services.AddEmappRequire(typeof(IDictMonitor<string>));
            if (hostConsumers)
            {
                TryAddMQHostedServices(services);

            }
            return services;
        }

        /// <summary>
        /// 注册MQConsumerService
        /// </summary>
        /// <param name="services"></param>
        private static void TryAddMQHostedServices(IServiceCollection services)
        {
            var libraries = DependencyContext.Default.RuntimeLibraries
                                .Where(d => DependencyHelper.IsEmappProject(d))
                                .Select(library => Assembly.Load(new AssemblyName(library.Name)))
                                .ToList();
            var deployOptions = GetDeployOptions();

            foreach (var assembly in libraries)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException te)
                {
                    types = te.Types;
                }
                foreach (var type in types)
                {
                    if (type != null && type.IsClass && !type.IsAbstract && type.BaseType.IsGenericType)
                    {
                        if (type.BaseType.GetGenericTypeDefinition() == typeof(MessageConsumer<>) && IsEnabled(type, deployOptions))
                        {
                            var consumerServiceDescriptor = ServiceDescriptor.Describe(type, type, type.GetServiceLife());
                            services.TryAdd(consumerServiceDescriptor);

                            var hostedServiceType = typeof(MessageConsumerService<>).MakeGenericType(type);
                            var hostedDescriptor = ServiceDescriptor.Singleton(typeof(IHostedService), hostedServiceType);
                            services.TryAddEnumerable(hostedDescriptor);
                        }
                    }
                }
            }
        }

        private static bool IsEnabled(Type type, DeployOptions deployOptions)
        {
            var options = type.GetCustomAttribute<MQConsumerOptionsAttribute>();
            if (options != null && !options.IsEnabled)
            {
                return false;
            }

            if (deployOptions?.Tags?.Count > 0)
            {
                var deployTag = options?.DeployTag;
                return deployOptions.Tags.Contains(deployTag, StringComparer.OrdinalIgnoreCase) ? deployOptions.IsEnabled : !deployOptions.IsEnabled;
            }
            return true;
        }

        private static DeployOptions GetDeployOptions()
        {
            var configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile(
                            path: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "appsettings.json"),
                            optional: true,
                            reloadOnChange: true)
                        .AddJsonFile(
                            path: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "emapp.json"),
                            optional: true,
                            reloadOnChange: true)
                        .Build();
            return configuration.GetSection("Emapp").Get<EmappSettings>()?.Deploy;
        }

        /// <summary>
        /// 根据特性设置ioc周期
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static ServiceLifetime GetServiceLife(this Type type)
        {
            if (type.GetCustomAttribute<ServiceLifeAttribute>() is ServiceLifeAttribute attr)
            {
                return attr.Mode switch
                {
                    ServiceLifeMode.Transient => ServiceLifetime.Transient,
                    ServiceLifeMode.Singleton => ServiceLifetime.Singleton,
                    ServiceLifeMode.Scoped => ServiceLifetime.Scoped,
                    _ => ServiceLifetime.Transient,
                };
            }
            return ServiceLifetime.Transient;
        }

    }
}
