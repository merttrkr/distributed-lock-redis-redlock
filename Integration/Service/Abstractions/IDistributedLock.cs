using RedLockNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Service.Abstractions;

public interface IDistributedLock
{
    public Task<bool> AcquireLockAsync();
}
