using Integration.Backend;
using Integration.Service.Abstractions;
using Integration.Service.Distributed;
using Integration.Service.Distributed.Redis;
using Integration.Test;
using Microsoft.Extensions.DependencyInjection;
using RedLockNet;
using RedLockNet.SERedis;
using System;

namespace Integration.Service
{
    public static class ServiceRegistration
    {
        public static void AddDistributedIntegrationServices(this IServiceCollection services)
        {
            // Register ItemOperationBackend
            services.AddSingleton<ItemOperationBackend>();

            // Register RedisConnectionFactory (Singleton - single instance for the entire application)
            services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();

            // Register RedLockFactory (Singleton - it's thread-safe)
            services.AddSingleton<RedLockFactory>(provider =>
            {
                var connectionFactory = provider.GetRequiredService<IRedisConnectionFactory>();
                var redisConnectionsTask = connectionFactory.CreateRedisConnectionsAsync();
                // Wait for the connections to be ready
                var redisConnections = redisConnectionsTask.Result.ToArray(); // Block here until task is completed
                return RedLockFactory.Create(redisConnections);
            });

            // Register RedisDistributedLock (Scoped - for each operation/request a new lock instance is used)
            services.AddScoped<RedisDistributedLock>(provider =>
            {
                var redLockFactory = provider.GetRequiredService<RedLockFactory>();

                // Define lock properties (adjust the expiry as needed)
                TimeSpan lockExpiry = TimeSpan.FromSeconds(10);

                return new RedisDistributedLock(redLockFactory, lockExpiry);
            });

            // Register ExponentialBackoff (Singleton - retry policy remains the same for all requests)
            services.AddSingleton<ExponentialBackoff>(provider =>
                new ExponentialBackoff(5, 2.0, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(100))
            );

            // Register the integration service
            services.AddScoped<IDistributedItemIntegrationService, DistributedItemIntegrationService>();

            // Register the test service for DI
            services.AddScoped<TestItemIntegrationServiceDistributed>();
        }
    }
}
