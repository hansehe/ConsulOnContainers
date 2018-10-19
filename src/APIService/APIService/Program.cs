using System;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace APIService
{
    public class Program
    {
        private static string Hostname => ConsulExtensions.InContainer ? Dns.GetHostName() : "localhost";

        private static int Port => ConsulExtensions.InContainer ? 80 : 5000;
        
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseUrls($"http://{Hostname}:{Port}")
                .UseStartup<Startup>();
    }
}