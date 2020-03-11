using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceDiscovery.LoadBalancer
{
    public class RandomLoadBalancer : ILoadBalancer
    {
        private readonly Random _random = new Random();
        public string Resolve(IList<string> services)
        {
            var index = _random.Next(services.Count);
            return services[index];
        }
    }
}
