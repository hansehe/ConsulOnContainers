using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace APIService
{
    public static class ConsulExtensions
    {
        public static bool InContainer => 
            Environment.GetEnvironmentVariable("RUNNING_IN_CONTAINER") == "true";
        
        public static IServiceCollection RegisterConsulClient(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = InContainer ? configuration["consulConfig:address"] : configuration["consulConfig:localAddress"];
                Console.WriteLine($"Consul Address: {address}");
                consulConfig.Address = new Uri(address);
            }));
            return services;
        }
        
        public static IApplicationBuilder RegisterWithConsul(this IApplicationBuilder app)
        {
            var consulClient = app.ApplicationServices
                .GetRequiredService<IConsulClient>();
            var configuration = app.ApplicationServices
                .GetRequiredService<IConfiguration>();
            var lifetime = app.ApplicationServices
                .GetRequiredService<IApplicationLifetime>();

            // Get server URI and IP address
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>();
            var address = addresses.Addresses.First();
            var uri = new Uri(address);
            var ip = Dns.GetHostEntry(uri.Host).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            
            // Get configured service name and id
            var serviceId = configuration["consulConfig:serviceID"];
            var serviceName = configuration["consulConfig:serviceName"];
            var serviceTags = configuration.GetSection("consulConfig:serviceTags").Get<List<string>>().ToArray();
            
            // AgentCheck - Possible health check options:
            //     - http://michaco.net/blog/ServiceDiscoveryAndHealthChecksInAspNetCoreWithConsul
            var tcpCheck = new AgentServiceCheck()
            {
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                Interval = TimeSpan.FromSeconds(2),
                TCP = $"{ip}:{uri.Port}"
            };
            
            var httpCheck = new AgentServiceCheck
            {
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                Interval = TimeSpan.FromSeconds(2),
                HTTP = $"{uri.Scheme}://{ip}:{uri.Port}/status"
            };
            
            // Register service with consul
            var registration = new AgentServiceRegistration
            {
                ID = $"{serviceId}-{uri.Port}",
                Name = serviceName,
                Address = $"{ip}",
                Port = uri.Port,
                Tags = serviceTags,
                Checks = new[] { tcpCheck, httpCheck }
            };

            Console.WriteLine("Registering with Consul");
            Console.WriteLine($"IP: {ip}");
            consulClient.Agent.ServiceDeregister(registration.ID).Wait();
            consulClient.Agent.ServiceRegister(registration).Wait();

            // Enable deregistration of service from consul during clean shutdown
            lifetime.ApplicationStopping.Register(() => {
                Console.WriteLine("Deregistering from Consul");
                consulClient.Agent.ServiceDeregister(registration.ID).Wait(); 
            });

            return app;
        }
    }
}