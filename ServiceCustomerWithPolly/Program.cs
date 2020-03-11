using ServiceDiscovery;
using ServiceDiscovery.LoadBalancer;
using System;
using System.Net.Http;
using System.Threading;

namespace ServiceCustomerWithPolly
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ConsulServiceProvider(new Uri("http://127.0.0.1:8500"));
            var myServiceA = serviceProvider.CreateServiceBuilder(config =>
            {
                config.ServiceName = "MyServiceA";
                config.LoadBalancer = TypeLoadBalancer.RoundRobin;
                config.UriScheme = Uri.UriSchemeHttp;
            });

            var httpClient = new HttpClient();
            var policy = PolicyBuilder.CreatePolly();

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"-------------调用第{i}次------------");
                policy.Execute(() =>
                {
                    try
                    {
                        var uri = myServiceA.BuildAsync("/api/order").Result;
                        Console.WriteLine($"{DateTime.Now}--正在调用：{uri}");
                        var content = httpClient.GetStringAsync(uri).Result;
                        Console.WriteLine($"结果：{content}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"异常：{e.Message}");
                        throw e;
                    }
                });

                Thread.Sleep(1000);
            }
        }
    }
}
