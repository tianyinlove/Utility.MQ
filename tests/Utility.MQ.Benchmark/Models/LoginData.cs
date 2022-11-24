
using Utility.RabbitMQ.Attributes;

namespace Utility.RabbitMQ.Benchmark
{
    [RabbitMQ("Emapp", "mqtest.login")]
    class LoginData
    {
        public string UserName { get; set; }
    }
}
