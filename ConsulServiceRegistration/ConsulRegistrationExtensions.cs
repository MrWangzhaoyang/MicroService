using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Consul;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;

namespace ConsulServiceRegistration
{
    // consul服务注册扩展类
    public static class ConsulRegistrationExtensions
    {
        public static void AddConsul(this IServiceCollection service)
        {
            // 读取服务配置文件
            var config = new ConfigurationBuilder().AddJsonFile("service.config.json").Build();
            service.Configure<ConsulServiceOptions>(config);
        }

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app)
        {
            Console.WriteLine("123");
            // 获取主机生命周期管理接口
            var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

            // 获取服务配置项
            var serviceOptions = app.ApplicationServices.GetRequiredService<IOptions<ConsulServiceOptions>>().Value;

            // 服务ID必须保证唯一
            serviceOptions.ServiceId = Guid.NewGuid().ToString();

            var consulClient = new ConsulClient(configuration =>
            {
                //服务注册的地址，集群中任意一个地址
                configuration.Address = new Uri(serviceOptions.ConsulAddress);
            });

            // 获取当前服务地址和端口，配置方式，也可以用自动获取
            var features = app.Properties["server.Features"] as FeatureCollection;
            var address = features.Get<IServerAddressesFeature>().Addresses.First();
            var uri = new Uri(address);

            // 节点服务注册对象
            var registration = new AgentServiceRegistration()
            {
                ID = serviceOptions.ServiceId,
                Name = serviceOptions.ServiceName,// 服务名
                Address = uri.Host,
                Port = uri.Port, // 服务端口
                Check = new AgentServiceCheck
                {
                    // 注册超时
                    Timeout = TimeSpan.FromSeconds(5),
                    // 服务停止多久后注销服务
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                    // 健康检查地址
                    HTTP = $"{uri.Scheme}://{uri.Host}:{uri.Port}{serviceOptions.HealthCheck}",
                    // 健康检查时间间隔
                    Interval = TimeSpan.FromSeconds(10),
                }
            };

            // 注册服务
            consulClient.Agent.ServiceRegister(registration).Wait();

            // 应用程序终止时，注销服务
            lifetime.ApplicationStopping.Register(() =>
            {
                consulClient.Agent.ServiceDeregister(serviceOptions.ServiceId).Wait();
            });

            return app;

        }
    }
}
