using Integration.Service.Abstractions;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Integration.Service.Distributed.Redis;

/// <summary>
/// Provides functionality to create and manage Redis connections using ConnectionMultiplexer for distributed operations.
/// Implements a singleton pattern to ensure only one connection is created.
/// </summary>
public class RedisConnectionFactory : IRedisConnectionFactory
{
    private readonly Lazy<Task<ConnectionMultiplexer>> _connectionMultiplexer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisConnectionFactory"/> class.
    /// Creates a lazy-loaded connection multiplexer for Redis nodes.
    /// </summary>
    public RedisConnectionFactory()
    {
        // Initialize the ConnectionMultiplexer as a singleton
        _connectionMultiplexer = new Lazy<Task<ConnectionMultiplexer>>(CreateConnectionAsync);
    }

    /// <summary>
    /// Creates and establishes a connection to the Redis nodes using ConnectionMultiplexer asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous connection creation. The task result is a <see cref="ConnectionMultiplexer"/> for Redis.</returns>
    private async Task<ConnectionMultiplexer> CreateConnectionAsync()
    {
        try
        {
            var configurationOptions = new ConfigurationOptions
            {
                EndPoints =
                {
                    "redis1:6379",
                    "redis2:6379",
                    "redis3:6379",
                    "redis4:6379",
                    "redis5:6379"
                },
                AbortOnConnectFail = false,
                Password = "your_password", // Ensure this is set securely, possibly via configuration
                ConnectRetry = 5,           // Retry connection up to 5 times
                SyncTimeout = 5000          // Set synchronous operation timeout to 5 seconds
            };

            var connection = await ConnectionMultiplexer.ConnectAsync(configurationOptions);
            return connection;
        }
        catch (RedisConnectionException ex)
        {
            // Handle connection errors (e.g., log the error)
            Console.WriteLine($"Redis connection failed: {ex.Message}");
            throw; // Re-throw the exception after logging
        }
        catch (Exception ex)
        {
            // Handle other exceptions (e.g., log the error)
            Console.WriteLine($"An error occurred while connecting to Redis: {ex.Message}");
            throw; // Re-throw the exception after logging
        }
    }

    /// <summary>
    /// Returns the singleton instance of the Redis connection multiplexer.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result is the <see cref="ConnectionMultiplexer"/> instance.</returns>
    public async Task<ConnectionMultiplexer> GetConnectionMultiplexerAsync()
    {
        return await _connectionMultiplexer.Value; // Return the singleton instance
    }

    /// <summary>
    /// Creates Redis connections for RedLock using the existing connection multiplexer.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result is a list of <see cref="RedLockMultiplexer"/> instances for RedLock.</returns>
    public async Task<IEnumerable<RedLockMultiplexer>> CreateRedisConnectionsAsync()
    {
        // Use a single connection multiplexer for all Redis nodes
        var connectionMultiplexer = await GetConnectionMultiplexerAsync();
        return new List<RedLockMultiplexer>
        {
            new RedLockMultiplexer(connectionMultiplexer)
        };
    }
}
