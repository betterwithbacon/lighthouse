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

            foreach (var serviceDescriptor in config.ServicesToRun)
            {
                var serviceToRun = LighthouseFetcher.Fetch(serviceDescriptor.Name, config.ServiceRepository, localDirectory);

                // inform the runtime to run the following services                
                // TODO: i'm not crazy about this, but it'll do for now
                lighthouseServer.Launch(serviceToRun.ServiceType);
            }

            var waitToKill = Console.ReadLine();

            // let it kill all processes
            await lighthouseServer.Stop();
        }
    }

    public static class LighthouseFetcher
    {
        public static ConcurrentBag<ILighthouseServiceDescriptor> AllFoundServices = new ConcurrentBag<ILighthouseServiceDescriptor>();

        public static ILighthouseServiceDescriptor Fetch(string serviceName, Uri serviceRepository, string currentDirectory)
        {
            // cache hit
            var foundService = AllFoundServices.FirstOrDefault(service => service.Name == serviceName);

            if (foundService != null)
            {
                return foundService;
            }
            else {
                // load local assemblies
                var dllTypeLoader = new DllTypeLoader();

                foreach (var service in 
                            dllTypeLoader.Load<ILighthouseService>(
                                currentDirectory, 
                                    (t) => 
                                    {
                                        var attrs = t.GetCustomAttributes(typeof(ExternalLighthouseServiceAttribute), false);
                                        return attrs != null && attrs.Length > 0;
                                    }
                                    ))
                {
                    var typeDescriptor = service.ToServiceDescriptor();
                    if (!AllFoundServices.Contains(typeDescriptor))
                    {
                        AllFoundServices.Add(typeDescriptor);
                        return typeDescriptor;
                    }
                }

                // load remote services, not found locally
                if (serviceRepository != null)
                {
                    
                }

                return null;
            }
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
