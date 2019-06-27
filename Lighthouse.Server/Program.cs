using CommandLine;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Lighthouse.Server
{
    class Program
    {
        [Verb("run")]
        public class RunOptions
        {
            [Option('a', "app", Required = true, HelpText = "Application name to run")]
            public string Application { get; set; }
        }

        [Verb("view")]
        public class ViewOptions
        {
            [Option('a', "app", Required = true, HelpText = "Application name to run")]
            public string Application { get; set; }

            [Option('s', "server", Required = true, HelpText = "Server application is hosted on", Default ="127.0.0.1:50505")]
            public string Server { get; set; }
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting lighthouse server");

            var server = new LighthouseServer(localLogger: Console.WriteLine);
            server.Start();

            Parser.Default.ParseArguments<RunOptions, ViewOptions>(args)
                .MapResult(
                    (RunOptions runOptions) =>
                    {
                        Type appType = LighthouseFetcher.Fetch(runOptions.Application);
                        if (appType == null)
                        {
                            throw new ApplicationException("Can't find app name");
                        }

                        server.Launch(appType);
                        return 0;
                    },
                    (ViewOptions viewOptions) =>
                    {
                        if(Uri.TryCreate(viewOptions.Server, UriKind.Absolute, out var uri))
                        {
                            var connection = server.Connect(uri);

                            // do more things here
                            return 0;
                        }
                        else
                        {
                            throw new ApplicationException($"URI: {viewOptions.Server} could not be converted to a valid URI");                            
                        }
                    },
                    errs => 1
                );
               
            _ = Console.ReadLine();
            await server.Stop();
        }
    }
}
