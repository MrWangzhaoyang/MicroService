using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceDiscovery.LoadBalancer
{
    public interface ILoadBalancer
    {
        string Resolve(IList<string> services);
    }
}
