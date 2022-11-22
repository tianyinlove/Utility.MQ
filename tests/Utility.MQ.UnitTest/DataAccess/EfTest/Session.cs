using Emapp.MQ.Core.Repositories;
using System;

namespace Emapp.MQ.UnitTest.DataAccess.EfTest
{
    /// <summary>
    /// 
    /// </summary>
    public class Session : IEntity<Guid>
    {

        ///<summary>
        /// Id (Primary key)
        ///</summary>
        public System.Guid Id { get; set; } = System.Guid.NewGuid();

        ///<summary>
        /// Name (length: 50)
        ///</summary>
        public string Name { get; set; }
    }
}
