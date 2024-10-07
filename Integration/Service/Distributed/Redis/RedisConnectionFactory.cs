using Integration.Service.Abstractions;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Integration.Service.Distributed.Redis;

/// <summary>
/// Provides functionality to create and manage Redis connections using ConnectionMultiplexer for distributed operations.
/// Implements a singleton pattern to ensure only one set of Redis connections is created for the application.
/// </summary>
public class RedisConnectionFactory : IRedisConnectionFactory
{
    // A list of RedLockMultiplexer objects representing Redis connections
    private readonly List<RedLockMultiplexer> _connectionMultiplexers;

    // A list of Redis endpoints used to establish connections
    private readonly List<string> _redisEndpoints = new List<string>
    {
        "redis1:6379",
        "redis2:6379",
        "redis3:6379",
        "redis4:6379",
        "redis5:6379"
    };

    /// <summary>
    /// Constructor that initializes the Redis connections synchronously by calling CreateRedisConnections().
    /// </summary>
    public RedisConnectionFactory()
    {
        // Initialize the ConnectionMultiplexer list when the class is instantiated
        _connectionMultiplexers = CreateRedisConnections();
    }

    /// <summary>
    /// Creates Redis connections based on the configured endpoints.
    /// Handles any connection exceptions and logs them, retrying connections up to the configured number of times.
    /// </summary>
    /// <returns>A list of RedLockMultiplexer objects representing Redis connections.</returns>
    private List<RedLockMultiplexer> CreateRedisConnections()
    {
        var connections = new List<RedLockMultiplexer>();

        foreach (var endpoint in _redisEndpoints)
        {
            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { endpoint },
                AbortOnConnectFail = false,     // Prevents app shutdown on failed connections
                Password = "your_password",     // Placeholder for secure password management
                ConnectRetry = 5,               // Retry failed connections up to 5 times
                AllowAdmin = true,              // Enables admin commands for lock debugging
                SyncTimeout = 5000              // Sets the synchronous timeout limit to 5 seconds
            };

            try
            {
                // Attempt to connect to the Redis server with the specified configuration
                var connection = ConnectionMultiplexer.Connect(configurationOptions);
                connections.Add(connection);
            }
            catch (RedisConnectionException ex)
            {
                // Log and rethrow the exception if connection fails
                Console.WriteLine($"Redis connection failed for {endpoint}: {ex.Message}");
                throw;
            }
        }

        return connections; // Return the list of successful Redis connections
    }

    /// <summary>
    /// Returns the list of ConnectionMultiplexer objects that represent the established Redis connections.
    /// This method is used to retrieve the connections for distributed locking.
    /// </summary>
    /// <returns>A list of RedLockMultiplexer objects.</returns>
    public List<RedLockMultiplexer> GetConnectionMultiplexers()
    {
        return _connectionMultiplexers; // Return the singleton list of Redis connections
    }
}
