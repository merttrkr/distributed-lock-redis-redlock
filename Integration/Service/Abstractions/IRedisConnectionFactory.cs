using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Integration.Service.Abstractions;

public interface IRedisConnectionFactory
{

    public List<RedLockMultiplexer> GetConnectionMultiplexers();


}