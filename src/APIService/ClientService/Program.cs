using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Consul;

namespace ClientService
{
    class Program
    {
        public static bool InContainer => 
            Environment.GetEnvironmentVariable("RUNNING_IN_CONTAINER") == "true";

        private static string ConsulHost => InContainer ? "consul-server-1-bootstrapper" : "localhost";
        
        static void Main(string[] args)
        {
            Thread.Sleep(TimeSpan.FromSeconds(3));
            var tags = new[] {"ApiService", "consulTesting", "ILuuveCaaS"};
            var serverUrls = GetServerUrls(tags);
            Console.WriteLine("Found following matching servers urls: ");
            foreach (var serverUrl in serverUrls)
            {
                Console.WriteLine(serverUrl);
            }
        }

        private static Uri[] GetServerUrls(string[] tags)
        {
            var serverUrls = new List<Uri>();
            var consulClient = new ConsulClient(c => c.Address = new Uri($"http://{ConsulHost}:8500"));
            var services = consulClient.Agent.Services().Result.Response;
            foreach (var service in services)
            {
                var isApi = service.Value.Tags.Any(t => tags.Any(tag => t == tag));
                if (isApi)
                {
                    var serviceUri = new Uri($"{service.Value.Address}:{service.Value.Port}");
                    serverUrls.Add(serviceUri);
                }
            }

            return serverUrls.ToArray();
        }
    }
}