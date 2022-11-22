using Utility.MQ.Attributes;

namespace Utility.MQ.UnitTest.Models
{
    [RabbitMQ("Emapp", "mqtest.login")]
    class LoginMessage
    {
        public string UserName { get; set; }
    }
}
