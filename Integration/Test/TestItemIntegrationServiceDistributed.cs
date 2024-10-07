using Integration.Service.Abstractions;
using Integration.Service.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Integration.Test;

public class TestItemIntegrationServiceDistributed
{
    private readonly IDistributedItemIntegrationService _distributedItemService;

    public TestItemIntegrationServiceDistributed(IDistributedItemIntegrationService distributedItemService)
    {
        _distributedItemService = distributedItemService;
    }

    public async Task TestAsync()
    {

        var itemContents = new List<string>
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
            await Task.Delay(Random.Shared.Next(0, 200)); // Simulate varied arrival times
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
