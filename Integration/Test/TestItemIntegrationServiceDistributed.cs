using Integration.Service.Abstractions;
using Integration.Service.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Integration.Test;

public class TestItemIntegrationServiceDistributed
{
    private readonly IAsyncIntegrationService _distributedItemService;

    public TestItemIntegrationServiceDistributed(IAsyncIntegrationService distributedItemService)
    {
        _distributedItemService = distributedItemService;
    }

    public async Task TestAsync()
    {
        // Test item content with duplicates to simulate multiple threads.
        var itemContents = new List<string>
        {
            "Content1",
            "Content2",
            "Content3",
            "Content4",
            "Content5",
        };

        var itemContents2 = new List<string>
        {
            "Content1",
            "Content2",
            "Content3",
            "Content1",
            "Content4",
            "Content5",
            "Content2"
        };


        // Step 1: Simulate parallel execution for itemContents list
        await Parallel.ForEachAsync(itemContents, async (content, _) =>
        {
            var result = await _distributedItemService.SaveItemAsync(content);
            Console.WriteLine(result.Message);
        });

        // Step 2: Simulate parallel execution for itemContents2 list
        await Parallel.ForEachAsync(itemContents2, async (content, _) =>
        {
            var result = await _distributedItemService.SaveItemAsync(content);
            Console.WriteLine(result.Message);
        });

        // Step 3: Check the saved items after the test
        var allItems = _distributedItemService.GetAllItems();
        Console.WriteLine("\nSaved Items:");
        foreach (var item in allItems)
        {
            Console.WriteLine($"Item ID: {item.Id}, Content: {item.Content}");
        }
    }
}
