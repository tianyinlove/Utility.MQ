using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Core.IData
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITestData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetAuth();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<bool> SetAuth(string name);
    }
}
