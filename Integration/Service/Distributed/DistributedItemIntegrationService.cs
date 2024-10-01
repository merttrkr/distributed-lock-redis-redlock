using Integration.Backend;
using Integration.Common;
using Integration.Service.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Integration.Service.Distributed;

public sealed class DistributedItemIntegrationService: IAsyncIntegrationService
{
    private readonly ItemOperationBackend _itemIntegrationBackend;
    private readonly RedisDistributedLock _redisLock;
    private readonly ExponentialBackoff _exponentialBackoff;

    public DistributedItemIntegrationService(
        ItemOperationBackend itemIntegrationBackend,
        RedisDistributedLock redisLock,
        ExponentialBackoff exponentialBackoff)
    {
        _itemIntegrationBackend = itemIntegrationBackend;
        _redisLock = redisLock;
        _exponentialBackoff = exponentialBackoff;
    }

    public async Task<Result> SaveItemAsync(string itemContent)
    {
        var lockKey = $"lock:item:{itemContent}";

        try
        {
            _redisLock.SetResource(lockKey); // Set lock key dynamically
            var lockAcquired = await _exponentialBackoff.RetryAsync(_redisLock.AcquireLockAsync);

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

            // Save item
            var item = _itemIntegrationBackend.SaveItem(itemContent);
            return new Result(true, $"Item with content '{itemContent}' saved successfully with ID {item.Id}.");
        }
        catch (Exception ex)
        {
            return new Result(false, $"Error saving item: {ex.Message}");
        }
        finally
        {
            _redisLock.Dispose(); // Ensure lock is released
        }
    }

    public List<Item> GetAllItems()
    {
        return _itemIntegrationBackend.GetAllItems();
    }
}
