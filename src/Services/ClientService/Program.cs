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
        private static bool InContainer => 
            Environment.GetEnvironmentVariable("RUNNING_IN_CONTAINER") == "true";

        private static string ConsulHost => InContainer ? "consul-server-1-bootstrapper" : "localhost";
        
        static void Main(string[] args)
        {
            // Continue polling consul and matching api services
            var tags = new[] {"ApiService", "consulTesting", "ILuuveCaaS"};
            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
                ClientTask(tags).Wait();
            }
        }

        private static async Task ClientTask(string[] tags)
        {
            var serverUris = await GetApiServerUris(tags);
            if (serverUris.Length <= 0) {
                return;
            }
            var serverUri = SelectRandomUri(serverUris);
            await PollApiServer(serverUri);
        }

        private static async Task PollApiServer(Uri uri)
        {
            var client = new HttpClient();
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

        private static Uri SelectRandomUri(Uri[] uris)
        {
            Console.WriteLine("Selecting a random uri from: ");
            foreach (var uri in uris)
            {
                Console.WriteLine($"\t - {uri}");
            }
            var random = new Random();
            var randomNumber = random.Next(0, uris.Length);
            return uris[randomNumber];
        }

        private static async Task<Uri[]> GetApiServerUris(string[] tags)
        {
            var serverUris = new List<Uri>();
            var consulClient = new ConsulClient(c => c.Address = new Uri($"http://{ConsulHost}:8500"));
            var services = (await consulClient.Agent.Services()).Response;
            foreach (var service in services)
            {
                var isApi = service.Value.Tags.All(t => tags.Any(tag => t == tag));
                if (!isApi)
                {
                    throw new Exception($"The tags did not match any registered services on Consul");
                }
                var serviceUri = new Uri($"http://{service.Value.Address}:{service.Value.Port}");
                serverUris.Add(serviceUri);
            }

            return serverUris.ToArray();
        }
    }
}