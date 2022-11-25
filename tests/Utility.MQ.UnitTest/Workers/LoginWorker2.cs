using Utility.RabbitMQ.UnitTest.Models;
using Utility.Dependency;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Utility.Core.Common;

namespace Utility.RabbitMQ.UnitTest.Workers
{
    class LoginWorker2 : MessageConsumer<LoginMessage>
    {
        private readonly AppSettings config;

        public LoginWorker2(IOptionsMonitor<AppSettings> optionsMonitor)
        {
            config = optionsMonitor.CurrentValue;
        }

        public override string ConsumerName => "emtest2";

        public override string ConsumerAppId => "Emapp";

        public override string RabbitMQConfig => config.CacheRabbitMQConfig;

        public override Task<bool> ExecuteAsync(LoginMessage message, MessageContext context)
        {
            //Trace.WriteLine($"comsume2 context:{context.ToJson()},param:{message.UserName}");
            //throw new System.Exception("eee");
            return Task.FromResult(true);
        }
    }
}
