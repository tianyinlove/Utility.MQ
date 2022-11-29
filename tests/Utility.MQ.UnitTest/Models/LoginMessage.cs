using Utility.RabbitMQ.Attributes;

namespace Utility.RabbitMQ.UnitTest.Models
{
    [RabbitMQ("Emapp", "mqtest.login", "RabbitMQConfig")]
    class LoginMessage
    {
        public string UserName { get; set; }
    }
}
