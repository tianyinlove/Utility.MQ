using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Core.ISevice
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITestService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isUpdate"></param>
        /// <returns></returns>
        Task<List<string>> GetAuth(bool isUpdate = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<bool> AuthUpdate(string userName);
    }
}
