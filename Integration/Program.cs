using Integration.Service;
using Integration.Service.local;
using Integration.Test;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Integration;

class Program
{

    static async Task Main(string[] args)
    {
        //TestItemIntegrationServiceLocal service = new();
        //service.Test();

        // Set up dependency injection
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDistributedIntegrationServices(); // Add your service registration
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Create an instance of the test class and run the test
        var testService = serviceProvider.GetRequiredService<TestItemIntegrationServiceDistributed>();

        // Run the test method
        await testService.TestAsync();
    }
}
