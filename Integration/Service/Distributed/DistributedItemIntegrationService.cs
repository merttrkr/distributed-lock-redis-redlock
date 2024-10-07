using Integration.Backend;
using Integration.Common;
using Integration.Service.Abstractions;
using Integration.Service.Distributed.Redis;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Service.Distributed;

/// <summary>
/// Service for distributed item integration, ensuring thread safety and distributed locking through Redis.
/// </summary>
public sealed class DistributedItemIntegrationService : IDistributedItemIntegrationService
{
    private readonly ItemOperationBackend _itemIntegrationBackend;
    private readonly RedisDistributedLock _redisLock;
    private readonly ExponentialBackoff _exponentialBackoff;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedItemIntegrationService"/> class.
    /// </summary>
    /// <param name="itemIntegrationBackend">Backend for item operations.</param>
    /// <param name="redisLock">Distributed lock for thread safety in Redis.</param>
    /// <param name="exponentialBackoff">Exponential backoff retry mechanism for acquiring the lock.</param>
    public DistributedItemIntegrationService(
        ItemOperationBackend itemIntegrationBackend,
        RedisDistributedLock redisLock,
        ExponentialBackoff exponentialBackoff)
    {
        _itemIntegrationBackend = itemIntegrationBackend;
        _redisLock = redisLock;
        _exponentialBackoff = exponentialBackoff;
    }

    /// <summary>
    /// Saves an item asynchronously, ensuring only one process can save the same item at a time using a Redis lock.
    /// </summary>
    /// <param name="itemContent">The content of the item to be saved.</param>
    /// <returns>A result indicating the success or failure of the operation.</returns>
    public async Task<Result> SaveItemAsync(string itemContent)
    {
        // Generate a more unique lock key
        string lockKey = GenerateUniqueLockKey(itemContent);

        try
        {
            // Set dynamic Redis lock resource key
            _redisLock.SetResource(lockKey);

            // Attempt to acquire lock with retry logic
            var lockAcquired = await _exponentialBackoff.RetryAsync(() => _redisLock.AcquireLockAsync());
            if (!lockAcquired)
            {
                return new Result(false, "Could not acquire Redis lock.");
            }

            // Check if item already exists
            var existingItems = _itemIntegrationBackend.FindItemsWithContent(itemContent);
            if (existingItems.Count > 0)
            {
                return new Result(false, $"Item with content '{itemContent}' already exists.");
            }

            // Save item to backend
            var item = _itemIntegrationBackend.SaveItem(itemContent);
            return new Result(true, $"Item with content '{itemContent}' saved successfully with ID {item.Id}.");
        }
        catch (Exception ex)
        {
            return new Result(false, $"Error saving item: {ex.Message}");
        }
        finally
        {
            // Ensure lock is released after operation
            _redisLock.Dispose();
        }
    }
    /// <summary>
    /// Generates a unique lock key using a hash of the content
    /// </summary>
    /// <returns>A lock key.</returns>

    private string GenerateUniqueLockKey(string itemContent)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(itemContent));
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            // Create a lock key with a namespace and the hashed content
            return $"app:lock:item:{hashString}";
        }
    }
    /// <summary>
    /// Retrieves all items from the backend.
    /// </summary>
    /// <returns>A list of all items.</returns>
    public List<Item> GetAllItems()
    {
        return _itemIntegrationBackend.GetAllItems();
    }
}
