using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Integration.Service.Abstractions;

public interface IRedisConnectionFactory
{
    public Task<ConnectionMultiplexer> GetConnectionMultiplexerAsync();
    public Task<IEnumerable<RedLockMultiplexer>> CreateRedisConnectionsAsync();


}