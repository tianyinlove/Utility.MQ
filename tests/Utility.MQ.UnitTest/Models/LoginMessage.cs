using Emapp.Attributes;

namespace Utility.MQ.UnitTest.Models
{
    [EmappMQ(Emapp.Constants.AppId.Emapp, "mqtest.login")]
    class LoginMessage
    {
        public string UserName { get; set; }
    }
}
