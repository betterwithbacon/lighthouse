using CommandLine;
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
    public class LighthouseServerLaunchOptions
    {
        public LighthouseServerLaunchOptions(string config, string app)
        {
            Config = config;
            App = app;
        }

        [Option('c', SetName="type")]
        public string Config { get; }

        [Option('a', SetName = "type")]
        public string App { get; }

    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Lighthouse Host starting...");

            LighthouseServer lighthouseServer = null;

            var result = Parser.Default.ParseArguments<LighthouseServerLaunchOptions>(args);
            var results = result
                .WithParsed(options =>
                {
                    lighthouseServer = LighthouseLauncher.Launch(options);
                });
                //.WithNotParsed((errors) => {
                //    foreach (var error in errors)
                //    {
                //        Console.WriteLine(error);
                //    }
                //});

            //var waitToKill = Console.ReadLine();

            //// let it kill all processes
            //await lighthouseServer.Stop();
        }
    }

    public static class LighthouseLauncher
    {
        public static LighthouseServer Launch(LighthouseServerLaunchOptions options)
        {
            var localDirectory = Directory.GetCurrentDirectory();            
            var server = new LighthouseServer(
                    localLogger: Console.WriteLine,
                    workingDirectory: localDirectory
                );

            if (!string.IsNullOrEmpty(options.Config))
            {
                var config = GetLighthouseHostConfig();
                var configFile = File.ReadAllLines(options.Config);
                
                server.Start();
                server.Launch(config.ServicesToRun.Select(s => new ServiceLaunchRequest(s.Name)));
            }
            else if (!string.IsNullOrEmpty(options.App))
            {
                server.Start();
                var serviceDescriptor = LighthouseFetcher.Fetch(options.App, null, localDirectory);

                if(serviceDescriptor != null)
                {                    
                    server.Launch(serviceDescriptor.ToServiceLaunchRequest());
                }
                else
                {
                    throw new ApplicationException($"Type '{options.App}' not found.");
                }
            }
            else
            {
                throw new ApplicationException($"must select either app or provide a path to a configuration path");
            }

            return server;
        }

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

    public static class LighthouseFetcher
    {
        public static ConcurrentBag<ILighthouseServiceDescriptor> AllFoundServices = new ConcurrentBag<ILighthouseServiceDescriptor>();

        public static ILighthouseServiceDescriptor Fetch(string serviceName, Uri serviceRepository, string currentDirectory)
        {
            // load local assemblies            
            foreach (var service in
                        DllTypeLoader.Load<ILighthouseService>(
                            currentDirectory,
                                (t) =>
                                {
                                    var attrs = t.CustomAttributes.Where(att => att.AttributeType.Name == typeof(ExternalLighthouseServiceAttribute).Name);
                                    if (attrs != null && attrs.Count() > 0)
                                    {
                                        if ((attrs.First().ConstructorArguments[0].Value as string).Equals(serviceName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }
                    ))
            {
                return service.ToServiceDescriptor();
            }

            // load remote services, not found locally
            if (serviceRepository != null)
            {

            }

            return null;

        }
    }

    public class LighthouseHostConfig
    {
        public string ServerName { get; set; }
        public Uri ServiceRepository { get; set; }
        public IEnumerable<ILighthouseServiceDescriptor> ServicesToRun { get; set; }
    }
}
