using Utility.RabbitMQ.Attributes;

namespace Utility.RabbitMQ.UnitTest.Models
{
    [RabbitMQ("Emapp", "mqtest.login")]
    class LoginMessage
    {
        public string UserName { get; set; }
    }
}
