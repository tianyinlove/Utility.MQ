using Emapp.Attributes;

namespace Utility.MQ.Benchmark
{
    [EmappMQ(Emapp.Constants.AppId.Emapp, "mqtest.login")]
    class LoginData
    {
        public string UserName { get; set; }
    }
}
