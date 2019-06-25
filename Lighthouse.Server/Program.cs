using CommandLine;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Services;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Lighthouse.Server
{
    class Program
    {
        public class Options
        {
            [Option('a', "app", Required = true, HelpText = "Application name to run")]
            public string Application { get; set; }
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting lighthouse server");

            var appName = "";

            Parser.Default.ParseArguments<Options>(args)
               .WithParsed<Options>(o =>
               {
                   appName = o.Application;
               });

            var server = new LighthouseServer(
                localLogger: Console.WriteLine);

            Type appType = LighthouseFetcher.Fetch(appName);

            if(appType == null)
            {
                throw new ApplicationException("Can't find app name");
            }

            server.Start();
            server.Launch(appType);
            _ = Console.ReadLine();
            await server.Stop();
        }
    }
}
