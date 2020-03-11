using System;
using ServiceDiscovery;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvier = new ConsulServiceProvider(new Uri("http://127.0.0.1:8500"));
            var services = serviceProvier.GetServicesAsync("MyServiceA").Result;
            foreach (var service in services)
            {
                Console.WriteLine(service);
            }
        }
    }
}
