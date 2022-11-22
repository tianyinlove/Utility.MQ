using Emapp.MQ.UnitTest.DataAccess.EfTest;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Emapp.MQ.Core.Repositories;

namespace Emapp.MQ.UnitTest.DataAccess
{
    /// <summary>
    /// 测试ef仓储
    /// </summary>
    public class TestEfGuidKeyRepository : BaseTest
    {
        /// <summary>
        /// 测试
        /// </summary>
        [Fact]
        public void TestGuidRepo()
        {
            var provider = CreateProvider();
            var repo = provider.GetService<IRepository<Guid, Session>>();

            var item1 = repo.GetOne(d => d.Name == "Test");

            var item3 = repo.GetById(item1.Id);

            var item2 = repo.GetOne(d => d.Name != "Test");


            var item4 = repo.GetOne(d => d.Name == "Test000");
        }

        /// <summary>
        /// 测试
        /// </summary>
        [Fact]
        public void TestCURD()
        {
            var provider = CreateProvider();
            var db = provider.GetService<IMQDbContext>();

            var repo = provider.GetService<IRepository<Guid, Session>>();


            repo.Add(new Session { Id = Guid.NewGuid(), Name = "testobject" });

            var item = repo.GetOne(d => d.Name == "testobject");
            Assert.True(item != null);

            var item2 = repo.GetById(item.Id);
            Assert.True(item2 == item);

            var list = repo.GetList(d => d.Name == "testobject");
            Assert.True(list.Count > 0 && list.Exists(d => d.Id == item.Id));


            repo.Update(item);
            item = repo.GetById(item.Id);

            repo.Delete(item);
            item = repo.GetById(item.Id);
            Assert.True(item == null);

        }
    }
}
