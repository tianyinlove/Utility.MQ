using Emapp.Constants;
using Utility.MQ.Constants;
using Emapp.Utility.Json;
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace Utility.MQ.Internal
{
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

        private async Task DisposePoolAsync()
        {
            if (_connection != null)
            {
                await Task.Delay(10000);
                _connection.Dispose();
            }

        }

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

        static readonly ConcurrentDictionary<AppId, RabbitConnectionPool> _appPoolMap = new();

        private static object __poolLocker = new();
        internal static RabbitConnectionPool GetPool(AppId appId, string configJson)
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

        public static RabbitChannelWrapper GetChannel(AppId appId, string configJson)
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
