using Integration.Service.Abstractions;
using RedLockNet;
using RedLockNet.SERedis;


namespace Integration.Service.Distributed;


public class RedisDistributedLock : IDisposable, IDistributedLock
{
    private readonly RedLockFactory _redLockFactory;
    private IRedLock _redLock;
    private string _resource;
    private readonly TimeSpan _lockExpiry;

    public RedisDistributedLock(RedLockFactory redLockFactory,  TimeSpan lockExpiry)
    {
        _redLockFactory = redLockFactory ?? throw new ArgumentNullException(nameof(redLockFactory));
        _lockExpiry = lockExpiry;
    }

    // Allow setting resource dynamically
    public void SetResource(string resource)
    {
        _resource = resource ?? throw new ArgumentNullException(nameof(resource));
    }

    // Attempts to acquire the lock
    public async Task<bool> AcquireLockAsync()
    {
        if (string.IsNullOrWhiteSpace(_resource))
        {
            throw new InvalidOperationException("Resource key must be set before acquiring a lock.");
        }

        _redLock = await _redLockFactory.CreateLockAsync(_resource, _lockExpiry);
        return _redLock.IsAcquired;
    }

    // Dispose only the lock itself, not the factory
    public void Dispose()
    {
        _redLock?.Dispose();
    }
}



