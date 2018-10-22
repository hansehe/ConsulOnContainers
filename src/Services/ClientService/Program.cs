using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
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
            // Continue polling consul and matching api services
            var tags = new[] {"ApiService", "consulTesting", "ILuuveCaaS"};
            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));
                ClientTask(tags).Wait();
            }
        }

        private static async Task ClientTask(string[] tags)
        {
            var serverUrls = await GetServerUrls(tags);
            await PollServerUrls(serverUrls);
        }

        private static async Task PollServerUrls(Uri[] uris)
        {
            var client = new HttpClient();
            foreach (var uri in uris)
            {
                var requestUri = new Uri(uri, "api/values");
                Console.WriteLine($"Polling api service: {requestUri}");
                var response = await client.GetAsync(requestUri);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Could not get api values from: {requestUri}");
                }
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Got api values from: {requestUri} with content: {content}");
            }
        }

        private static async Task<Uri[]> GetServerUrls(string[] tags)
        {
            var serverUrls = new List<Uri>();
            var consulClient = new ConsulClient(c => c.Address = new Uri($"http://{ConsulHost}:8500"));
            var services = (await consulClient.Agent.Services()).Response;
            foreach (var service in services)
            {
                var isApi = service.Value.Tags.All(t => tags.Any(tag => t == tag));
                if (!isApi)
                {
                    throw new Exception($"No tags matched any registered services on Consul");
                }
                var serviceUri = new Uri($"http://{service.Value.Address}:{service.Value.Port}");
                serverUrls.Add(serviceUri);
            }

            return serverUrls.ToArray();
        }
    }
}