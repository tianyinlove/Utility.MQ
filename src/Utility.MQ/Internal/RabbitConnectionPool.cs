using Utility.MQ.Constants;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using Utility.Extensions;

namespace Utility.MQ.Internal
{
    /// <summary>
    /// 
    /// </summary>
    internal class RabbitConnectionPool
    {
        private readonly ConcurrentQueue<IModel> _connectionChannelPool = new();
        private IConnection _connection;
        private readonly object _locker = new();

        public RabbitConnectionPool(string configJson)
        {
            ConfigJson = configJson;
        }

        private string ConfigJson { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task DisposePoolAsync()
        {
            if (_connection != null)
            {
                await Task.Delay(10000);
                _connection.Dispose();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IModel GetChannel()
        {
            if (_connectionChannelPool.TryDequeue(out IModel channel))
            {
                if (channel.IsOpen)
                {
                    return channel;
                }
                else
                {
                    channel.Dispose();
                }
            }
            if (_connection == null)
            {
                lock (_locker)
                {
                    _connection ??= ConfigJson.FromJson<ConnectionFactory>().CreateConnection();
                }
            }

            channel = _connection.CreateModel();
            channel.ExchangeDeclare(exchange: ExchangeNames.MainExchange, ExchangeType.Direct, durable: true);
            channel.ExchangeDeclare(exchange: ExchangeNames.DelayExchange, ExchangeType.Headers, durable: true);
            return channel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        internal void ReleaseChannel(IModel channel)
        {
            if (channel != null)
            {
                if (channel.IsOpen)
                {
                    _connectionChannelPool.Enqueue(channel);
                }
                else
                {
                    channel.Dispose();
                }
            }
        }

        #region PoolManager

        static readonly ConcurrentDictionary<string, RabbitConnectionPool> _appPoolMap = new();

        private static object __poolLocker = new();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="configJson"></param>
        /// <returns></returns>
        internal static RabbitConnectionPool GetPool(string appId, string configJson)
        {
            if (!_appPoolMap.TryGetValue(appId, out RabbitConnectionPool pool) || pool.ConfigJson != configJson)
            {
                lock (__poolLocker)
                {
                    _appPoolMap.TryGetValue(appId, out pool);
                    if (pool == null)
                    {
                        _appPoolMap[appId] = pool = new RabbitConnectionPool(configJson);
                    }
                    else if (pool.ConfigJson != configJson)
                    {
                        _appPoolMap.TryRemove(appId, out RabbitConnectionPool oldPool);
                        _ = oldPool?.DisposePoolAsync();
                        _appPoolMap[appId] = pool = new RabbitConnectionPool(configJson);
                    }
                }
            }

            return pool;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="configJson"></param>
        /// <returns></returns>
        public static RabbitChannelWrapper GetChannel(string appId, string configJson)
        {
            var pool = GetPool(appId, configJson);
            return new RabbitChannelWrapper
            {
                Channel = pool.GetChannel(),
                Pool = pool,
            };
        }
        #endregion
    }
}
