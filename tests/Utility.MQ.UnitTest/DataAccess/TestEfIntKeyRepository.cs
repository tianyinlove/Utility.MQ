using Emapp.MQ.UnitTest.DataAccess.EfTest;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Emapp.MQ.Core.Repositories;

namespace Emapp.MQ.UnitTest.DataAccess
{
    /// <summary>
    /// 测试ef仓储
    /// </summary>
    public class TestEfIntKeyRepository : BaseTest
    {
        /// <summary>
        /// 测试
        /// </summary>
        [Fact]
        public void TestIntRepo()
        {
            var provider = CreateProvider();
            var repo = provider.GetService<IRepository<Basket>>();

            var item = repo.GetById(1);

            var item0 = repo.GetById(0);

            var item2 = repo.GetOne(d => d.Name == item.Name);

            var item3 = repo.GetOne(d => d.Name != item.Name);
        }

    }
}
