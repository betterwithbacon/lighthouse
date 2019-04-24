using Lighthouse.Core;
using Lighthouse.Core.Configuration.Formats.Memory;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lighthouse.Server.Host
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Lighthouse Host starting...");

            // load local config    

            if(args.Length < 2)
            {
                throw new ApplicationException("Configuration file not loaded");
            }

            var configFile = File.ReadAllLines(args[1]);
            
            var config = LighthouseLoader.GetLighthouseHostConfig();
            
            // create runtime
            var lighthouseServer = new LighthouseServer(
                    serverName: config.ServerName,
                    localLogger: Console.WriteLine,
                    workingDirectory: Directory.GetCurrentDirectory()                
                );

            // go to the service repository
            // load the assemblies locally, and expose them as types within this domain
            // TODO: should the lighthouse container be able to fetch the type and launch it, for now, lets just give the container the types
            var localDirectory = Directory.GetCurrentDirectory();            
            var servicesToRun = LighthouseFetcher.Fetch(config.ServiceRepository,localDirectory, config.ServicesToRun);

            // inform the runtime to run the following services
            foreach (var service in servicesToRun)
            {
                lighthouseServer.Launch(service);
            }

            var waitToKill = Console.ReadLine();

            // let it kill all processes
            await lighthouseServer.Stop();
        }
    }

    public static class LighthouseFetcher
    {
        public static ConcurrentBag<ILighthouseService> AllFoundServices = new ConcurrentBag<ILighthouseService>();

        public static IEnumerable<ILighthouseService> Fetch(Uri serviceRepository, string currentDirectory, IEnumerable<ILighthouseServiceDescriptor> servicesToRun)
        {
            var services = new List<ILighthouseService>();
            
            // load local assemblies
            var dllTypeLoader = new DllTypeLoader();

            foreach(var foundService in dllTypeLoader.Load<ILighthouseService>(currentDirectory))
            {
                if(!AllFoundServices.Contains(foundService))
                {
                    AllFoundServices.Add(foundService);
                }
            }
            
            // load remote services, not found locally
            if (serviceRepository != null)
            {

            }

            return services;
        }
    }

    public static class LighthouseLoader
    {
        public static LighthouseHostConfig GetLighthouseHostConfig()
        {
            // bunch of test stuff
            var config = new LighthouseHostConfig
            {
                ServerName = "Local Lighthouse Host"
            };

            if (Uri.TryCreate("localhost", UriKind.Absolute, out var repo))
                config.ServiceRepository = repo;
            else
            {
                UriBuilder uriBuilder = new UriBuilder("http", "127.0.0.1", 50505);

                config.ServiceRepository = uriBuilder.Uri;
            }
            config.ServicesToRun = new[] {
                new ServiceDescriptor {  Name = "test_app", Version = Version.Parse("1.0.0") }
            };
            return config;
        }
    }

    public class LighthouseHostConfig
    {
        public string ServerName { get; set; }
        public Uri ServiceRepository { get; set; }
        public IEnumerable<ILighthouseServiceDescriptor> ServicesToRun { get; set; }
    }
}
