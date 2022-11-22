using Utility.MQ.Services;
using Utility.Dependency;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using Utility.MQ;

namespace Utility.Extensions
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
        public static IServiceCollection AddMQService(this IServiceCollection services, bool hostConsumers = true)
        {
            return services.AddMQProducer().AddMQConsumer(hostConsumers);
        }

        /// <summary>
        /// 添加MQ服务客户端
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMQProducer(this IServiceCollection services)
        {
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
        public static IServiceCollection AddMQConsumer(this IServiceCollection services, bool hostConsumers = true)
        {
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
            var libraries = AppDomain.CurrentDomain.GetAssemblies();
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
                    if (type != null && type.IsClass && !type.IsAbstract && type.BaseType != null && type.BaseType.IsGenericType)
                    {
                        if (type.BaseType.GetGenericTypeDefinition() == typeof(MessageConsumer<>))
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
