using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDiscovery
{
    public interface IServiceProvider
    {
        Task<IList<string>> GetServicesAsync(string serviceName);
    }
}
