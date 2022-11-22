using Emapp.MQ.Core.Data;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Emapp.Configuration.Model;

namespace Emapp.MQ.UnitTest.DataAccess
{
    /// <summary>
    /// 测试功能
    /// </summary>
    public class Todo : BaseTest
    {
        /// <summary>
        /// 一个测试
        /// 测试命名:明确地表达测试的意图
        /// 要测试的方法的名称和方案_调用方案时的预期行为
        /// </summary>
        [Fact]
        public void GetString_ReturnNotNull()
        {
            //Arrange,准备
            var provider = CreateProvider();
            var service = provider.GetService<ITodoData>();

            //Act,运行
            var result = service.GetString();

            //Assert,断言
            Assert.True(!string.IsNullOrWhiteSpace(result));
        }

        /// <summary>
        /// 多次测试(顺序随机)
        /// 测试命名:明确地表达测试的意图
        /// 要测试的方法的名称和方案_调用方案时的预期行为
        /// </summary>
        /// <param name="value">参数</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public async Task Copy_ReturnSameValue(int value)
        {
            //Arrange,准备
            var host = await StartHostAsync();
            var service = host.Services.GetService<ITodoData>();
            //var urlConfig = host.Services.GetService<IOptionsMonitor<UrlConfig>>().CurrentValue;

            //Act,运行
            var result = service.Copy(value);

            //Assert,断言
            Assert.True(result == value);

            //清理
            await Task.Delay(5000);
            await StopHostAsync();
        }


        public static IEnumerable<object[]> MonitListParameters { get; }
            = new List<object[]> {
                new object[]{ new List<string> { "002060", "002070"} }
            };

        /// <summary>
        /// 智能监控
        /// </summary>
        [Theory]
        [MemberData(nameof(MonitListParameters))]
        public async Task GetStockMonitList(List<string> stockCodes)
        {
            //Arrange,准备
            var provider = CreateProvider();
            var service = provider.GetService<IZyNewsData>();

            //Act,运行
            var result = await service.GetStockMonitListAsync(stockCodes);

            //Assert,断言
        }
    }
}
