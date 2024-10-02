using Integration.Service.Abstractions;
using RedLockNet;
using RedLockNet.SERedis;

namespace Integration.Service.Distributed.Redis;

/// <summary>
/// Implements a distributed lock using Redis and RedLock for managing concurrent access to shared resources.
/// </summary>
public class RedisDistributedLock : IDisposable, IDistributedLock
{
    private readonly RedLockFactory _redLockFactory;
    private IRedLock _redLock;
    private string _resource;
    private readonly TimeSpan _lockExpiry;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisDistributedLock"/> class.
    /// </summary>
    /// <param name="redLockFactory">The RedLock factory used to create Redis-based locks.</param>
    /// <param name="lockExpiry">The expiry time for the lock.</param>
    public RedisDistributedLock(RedLockFactory redLockFactory, TimeSpan lockExpiry)
    {
        _redLockFactory = redLockFactory ?? throw new ArgumentNullException(nameof(redLockFactory));
        _lockExpiry = lockExpiry;
    }

    /// <summary>
    /// Sets the resource key for which the lock will be acquired.
    /// </summary>
    /// <param name="resource">The unique resource identifier (key) for the lock.</param>
    public void SetResource(string resource)
    {
        _resource = resource ?? throw new ArgumentNullException(nameof(resource));
    }

    /// <summary>
    /// Attempts to asynchronously acquire the Redis-based distributed lock.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result is <c>true</c> if the lock is acquired, otherwise <c>false</c>.</returns>
    public async Task<bool> AcquireLockAsync()
    {
        if (string.IsNullOrWhiteSpace(_resource))
        {
            throw new InvalidOperationException("Resource key must be set before acquiring a lock.");
        }

        _redLock = await _redLockFactory.CreateLockAsync(_resource, _lockExpiry);
        return _redLock.IsAcquired;
    }

    /// <summary>
    /// Releases the lock and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        _redLock?.Dispose();
    }
}
