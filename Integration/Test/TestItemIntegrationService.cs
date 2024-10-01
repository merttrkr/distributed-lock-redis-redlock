using Integration.Service.local;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Test;

public class TestItemIntegrationServiceLocal
{
    public void Test()
    {
        var service = new ItemIntegrationService();

        // Duplicate content containing list.
        var itemContents = new List<string>
        {
            "Content1",
            "Content2",
            "Content3",
            "Content1",  // Duplicate content
            "Content4",
            "Content5",
            "Content2"   // Duplicate content
        };
        // Duplicate list
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

        // each thread calls service layer in parallel for each loop.
        Parallel.ForEach(itemContents, content =>
        {
            var result = service.SaveItem(content);
            Console.WriteLine(result.Message);
        });

        Parallel.ForEach(itemContents2, content =>
        {
            var result = service.SaveItem(content);
            Console.WriteLine(result.Message);
        });
        // Check the saved items
        var allItems = service.GetAllItems();
        Console.WriteLine("\nSaved Items:");
        foreach (var item in allItems)
        {
            Console.WriteLine(item);
        }
    }
}
