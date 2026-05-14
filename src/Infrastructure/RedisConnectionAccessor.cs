using StackExchange.Redis;

namespace SecureCodingDemo.Infrastructure;

public sealed class RedisConnectionAccessor
{
    public RedisConnectionAccessor(IConnectionMultiplexer? connection)
    {
        Connection = connection;
    }

    public IConnectionMultiplexer? Connection { get; }

    public bool IsAvailable => Connection?.IsConnected == true;
}
