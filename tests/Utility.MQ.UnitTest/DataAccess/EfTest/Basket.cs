using Emapp.MQ.Core.Repositories;

namespace Emapp.MQ.UnitTest.DataAccess.EfTest
{
    
    /// <summary>
    /// 
    /// </summary>
    public class Basket : IEntity<int>
    {

        ///<summary>
        /// Id (Primary key)
        ///</summary>
        public int Id { get; set; }

        ///<summary>
        /// Name (length: 50)
        ///</summary>
        public string Name { get; set; }
    }
}
