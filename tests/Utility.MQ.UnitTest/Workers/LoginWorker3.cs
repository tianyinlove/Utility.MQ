using Emapp.Constants;
using Utility.MQ.UnitTest.Models;
using System;
using System.Threading.Tasks;

namespace Utility.MQ.UnitTest.Workers
{
    class LoginWorker3 : MessageConsumer<LoginMessage>
    {
        public LoginWorker3()
        {
        }

        public override string ConsumerName => "emtest3";

        public override AppId ConsumerAppId => AppId.Emapp;

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
