using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Utility.Extensions;

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
            var _assembly = Assembly.GetExecutingAssembly();
            return services.AddAssembly(_assembly);
        }

    }
}
