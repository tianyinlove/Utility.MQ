
using Utility.MQ.Attributes;

namespace Utility.MQ.Benchmark
{
    [RabbitMQ("Emapp", "mqtest.login")]
    class LoginData
    {
        public string UserName { get; set; }
    }
}
