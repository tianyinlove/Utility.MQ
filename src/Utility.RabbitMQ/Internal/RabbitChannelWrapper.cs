using RabbitMQ.Client;

namespace Utility.RabbitMQ.Internal;

/// <summary>
/// 
/// </summary>
internal class RabbitChannelWrapper : IDisposable
{
    public IModel Channel { get; internal set; }

    public RabbitConnectionPool Pool { get; internal set; }

    public void Dispose()
    {
        Pool?.ReleaseChannel(Channel);
    }
}
