using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ServiceDiscovery.LoadBalancer;

namespace ServiceDiscovery.Builder
{
    public interface IServiceBuilder
    {
        //服务提供者
        IServiceProvider ServiceProvider { get; set; }

        //服务名称
        string ServiceName { get; set; }

        //Uri方案
        string UriScheme { get; set; }

        //策略
        ILoadBalancer LoadBalancer { get; set; }

        Task<Uri> BuildAsync(string path);
    }
}
