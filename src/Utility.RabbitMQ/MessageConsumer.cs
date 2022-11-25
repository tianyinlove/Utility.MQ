using Utility.RabbitMQ.Constants;
using Utility.Extensions;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Utility.RabbitMQ
{
    /// <summary>
    /// 消息消费者
    /// </summary>
    public abstract class MessageConsumer<TMessage> : IMessageConsumer
    {
        /// <summary>
        /// MQ配置，Json格式
        /// </summary>
        public virtual string RabbitMQConfig { get; }

        /// <summary>
        /// 消费模块名
        /// </summary>
        public abstract string ConsumerName { get; }

        /// <summary>
        /// 消费者应用id
        /// </summary>
        public abstract string ConsumerAppId { get; }

        /// <summary>
        /// 
        /// </summary>
        public virtual ushort PrefetchCount { get; } = 10;

        /// <summary>
        /// 
        /// </summary>
        public virtual ushort PrefetchSize { get; } = 0;

        /// <summary>
        /// 队列重试次数
        /// </summary>
        public virtual int MaxRetry { get; } = 50;

        /// <summary>
        /// 本地重试次数
        /// </summary>
        public virtual int MaxLocalRetry { get; } = 1;

        /// <summary>
        /// 队列重试的延迟时间(ms)
        /// </summary>
        public virtual int GetRetryDelay(int failCount, MessageContext context)
        {
            return failCount switch
            {
                <= 1 => 1000,
                2 => 2000,
                > 2 and <= 5 => 5000,
                _ => 30000,
            };
        }

        /// <summary>
        /// 处理解析过的消息，返回消费是否成功
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task<bool> ExecuteAsync(TMessage message, MessageContext context);

        /// <summary>
        /// 处理mq原始消息
        /// </summary>
        /// <param name="body"></param>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public async Task<ExecuteResult> ExecuteMessageAsync(byte[] body, MessageContext context, ILogger logger)
        {
            TMessage message;
            string messageJson;

            try
            {
                messageJson = Encoding.UTF8.GetString(body);
                message = messageJson.FromApiJson<TMessage>();
            }
            catch (Exception err)
            {
                logger.LogWarning(err, "MQ消息 内容读取异常");
                return new ExecuteResult { ResultCode = ExecuteResultCode.BadBody, ErrorMessage = err.Message };
            }

            logger.LogInformation($"MQ消息 收到消息,queue:{context.QueueName},json:{messageJson}");
            var maxRetry = Math.Max(1, MaxLocalRetry);
            if (context.RoutingKey == context.QueueName) // 处理重试队列
            {
                maxRetry = 1;
            }
            context.LocalFailCount = 0;
            for (int i = 0; i < maxRetry; i++)
            {
                try
                {
                    if (await ExecuteAsync(message, context))
                    {
                        return new ExecuteResult { ResultCode = ExecuteResultCode.Success };
                    }
                }
                catch (DropMessageException)
                {
                    return new ExecuteResult { ResultCode = ExecuteResultCode.Success };
                }
                catch (Exception err)
                {
                    logger.LogError(err, $"消费MQ消息异常");
                }
                context.LocalFailCount++;
            }

            context.FailCount++;
            if (context.FailCount < MaxRetry)
            {
                int delay;
                try
                {
                    delay = GetRetryDelay(context.FailCount, context);
                }
                catch (Exception)
                {
                    delay = 30000;
                }
                return new ExecuteResult { ResultCode = ExecuteResultCode.Retry, Delay = delay };
            }
            else
            {
                return new ExecuteResult { ResultCode = ExecuteResultCode.Fail };
            }
        }
    }
}
