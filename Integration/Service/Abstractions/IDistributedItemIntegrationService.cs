using Integration.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Service.Abstractions;

public interface IDistributedItemIntegrationService
{
    public Task<Result> SaveItemAsync(string itemContent);
    public List<Item> GetAllItems();


}
