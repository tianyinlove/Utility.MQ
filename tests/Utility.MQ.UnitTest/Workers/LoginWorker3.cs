using Utility.RabbitMQ.UnitTest.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Utility.Core.Common;

namespace Utility.RabbitMQ.UnitTest.Workers
{
    class LoginWorker3 : MessageConsumer<LoginMessage>
    {
        private readonly AppSettings config;

        public LoginWorker3(IOptionsMonitor<AppSettings> optionsMonitor)
        {
            config = optionsMonitor.CurrentValue;
        }

        public override string ConsumerName => "emtest3";

        public override string ConsumerAppId => "Emapp";
        public override string RabbitMQConfig => config.RabbitMQConfig;
        public override int MaxRetry => 10;

        public override int GetRetryDelay(int failCount, MessageContext context)
        {
            return 1000;
        }

        public override Task<bool> ExecuteAsync(LoginMessage message, MessageContext context)
        {
            //Trace.WriteLine($"comsume3 {context.QueueName} {context.MessageId},param:{message.UserName}");
            //throw new System.Exception("eee");
            //return Task.FromResult(MessageConsumeResult.Success);

            return Task.FromResult(context.FailCount >= 5 && new Random().Next(0, 10) == 0);
        }
    }
}
