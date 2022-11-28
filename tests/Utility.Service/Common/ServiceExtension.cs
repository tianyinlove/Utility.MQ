using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Utility.Dependency;
using Utility.Extensions;
using Utility.RabbitMQ;

namespace Utility.Core.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class ServiceExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUtilityTestService(this IServiceCollection services)
        {
            var typeList = DependencyHelper.FindServices();
            foreach (var item in typeList)
            {
                services.TryAdd(item);
            }

            //var libraries = AppDomain.CurrentDomain.GetAssemblies();
            //foreach (var assembly in libraries)
            //{
            //    Type[] types;
            //    try
            //    {
            //        types = assembly.GetTypes();
            //    }
            //    catch (ReflectionTypeLoadException te)
            //    {
            //        types = te.Types;
            //    }
            //    foreach (var type in types)
            //    {
            //        if (type != null && type.IsClass && !type.IsAbstract && !type.IsSealed && type.BaseType != null && type.BaseType.IsGenericType)
            //        {
            //            var iface = type.GetInterfaces();
            //            if (iface != null && iface.Length > 0)
            //            {
            //                var ifaceLst = iface.ToList();
            //                ifaceLst.ForEach(It =>
            //                {
            //                    if (It != null)
            //                    {
            //                        services.AddTransient(It, type);
            //                    }
            //                });
            //            }
            //        }
            //    }
            //}

            return services;
        }

    }
}
