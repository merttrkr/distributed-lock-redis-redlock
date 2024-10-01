using Integration.Service;
using System.Diagnostics;

namespace Integration
{
    class Program
    {
        static void Main(string[] args)
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


            // each thread calls service layer in parallel for each loop.
            Parallel.ForEach(itemContents, content =>
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
}
