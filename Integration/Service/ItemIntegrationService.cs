using Integration.Common;
using Integration.Backend;
using System.Collections.Concurrent;

namespace Integration.Service
{
    public sealed class ItemIntegrationService
    {
        // Dependency that is normally fulfilled externally.
        private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();

        // Dictionary to track ongoing saves.
        private ConcurrentDictionary<string, bool> ProcessedDictionary = new();

        // This is called externally and can be called multithreaded, in parallel.
        public Result SaveItem(string itemContent)
        {
            // Ensure no duplicate content is processed concurrently.
            if (!ProcessedDictionary.TryAdd(itemContent, true))
            {
                return new Result(false, $"Duplicate item detected for content '{itemContent}'.");
            }

            try
            {
                // Check if the item was already saved in the backend to ensure no duplicates.
                var existingItems = ItemIntegrationBackend.FindItemsWithContent(itemContent);
                if (existingItems.Count > 0)
                {
                    return new Result(false, $"Item with content '{itemContent}' already exists and will not be saved again.");
                }

                // Save the item if it has not been saved before.
                var item = ItemIntegrationBackend.SaveItem(itemContent);
                return new Result(true, $"Item with content '{itemContent}' saved successfully with ID {item.Id}.");
            }
            catch (Exception ex)
            {
                return new Result(false, $"An error occurred while saving the item: {ex.Message}");
            }
            finally
            {
                // Always remove the item from the processed dictionary after processing.
                ProcessedDictionary.TryRemove(itemContent, out _);
            }
        }

        public List<Item> GetAllItems()
        {
            return ItemIntegrationBackend.GetAllItems();
        }
    }
}
