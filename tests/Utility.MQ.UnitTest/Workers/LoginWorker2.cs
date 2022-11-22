using Emapp.Attributes;
using Emapp.Constants;
using Utility.MQ.UnitTest.Models;
using Emapp.Utility.Dependency;
using Emapp.Utility.Json;
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

        public override AppId ConsumerAppId => AppId.Emapp;

        public override Task<bool> ExecuteAsync(LoginMessage message, MessageContext context)
        {
            //Trace.WriteLine($"comsume2 context:{context.ToJson()},param:{message.UserName}");
            //throw new System.Exception("eee");
            return Task.FromResult(true);
        }
    }
}
