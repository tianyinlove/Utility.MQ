using Utility.MQ.UnitTest.Models;
using Utility.Dependency;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Utility.MQ.UnitTest.Workers
{
    class LoginWorker2 : MessageConsumer<LoginMessage>
    {
        public LoginWorker2()
        {
        }

        public override string ConsumerName => "emtest2";

        public override string ConsumerAppId => "Emapp";

        public override Task<bool> ExecuteAsync(LoginMessage message, MessageContext context)
        {
            //Trace.WriteLine($"comsume2 context:{context.ToJson()},param:{message.UserName}");
            //throw new System.Exception("eee");
            return Task.FromResult(true);
        }
    }
}
