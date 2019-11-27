using CommandLine;
using Lighthouse.Client;
using Lighthouse.Core;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.Utils;
using Lighthouse.Server;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Lighthouse.CLI
{
    class Program
    {
        public abstract class BaseLighthouseOptions
        {
            [Option("what", Required = true, HelpText = "What to do?")]
            public string What { get; set; }
            [Option("where", Required = false, HelpText = "Where to run it?")]
            public string Where { get; set; }
            [Option("how", Required = false, HelpText = "JSON payload sent to the action to run.")]
            public string How { get; set; }
        }

        [Verb("run")]
        public class RunOptions : BaseLighthouseOptions
        {
            [Option('f', "file", Required = false, HelpText = "Application configuration file to load")]
            public string File { get; set; }

            public bool IsFileMode => !string.IsNullOrEmpty(File);
        }

        [Verb("inspect")]
        public class InspectOptions : BaseLighthouseOptions
        {
        }

        [Verb("ping")]
        public class PingOptions : BaseLighthouseOptions
        {
        }

        // I'm not sure if this one needs to exist at this level, Ping seems like it can do a lot of the same things
        [Verb("benchmark")]
        public class BenchmarkOptions : BaseLighthouseOptions
        {
        }

        static async Task Main(string[] args)
        {            
            var result  = Parser.Default.ParseArguments<RunOptions, InspectOptions>(args)
                .MapResult(
                    (RunOptions run) =>
                    {
                        if (run.IsFileMode)
                        {
                            var fileContents = File.ReadAllText(run.File);

                            var config = YamlUtil.ParseYaml<LighthouseRunConfig>(fileContents);

                            

                            // load resources first
                            foreach (var resource in config.Resources)
                            {

                            }

                            // then launch apps
                            foreach (var app in config.Applications)
                            {

                            }
                        }
                        else
                        {
                            if(run.Where != null)
                            {
                                var client = new LighthouseClient(run.Where.ToUri());
                                client.AddLogger(Console.WriteLine);
                                // make a connection to the other serer
                                var response = client.MakeRequest<RemoteAppRunRequest, RemoteAppRunHandle>(new RemoteAppRunRequest(run.What)).GetAwaiter().GetResult();
                                Console.WriteLine($"Task was {response.Status} (ID: {response.Id})");
                            }
                            else
                            {
                                Type appType = LighthouseFetcher.Fetch(run.What);
                                if (appType == null)
                                {
                                    throw new ApplicationException($"Can't find app with name: {run.What}");
                                }

                                // start a lighthouse server locally, and have it run the task
                                var server = new LighthouseServer();
                                server.AddLogger(Console.WriteLine);
                                server.Launch(appType).GetAwaiter().GetResult();
                            }
                        }

                        return 0;
                    },
                    (InspectOptions inspect) =>
                    {
                        var client = new LighthouseClient(inspect.Where.ToUri());
                        client.AddLogger(Console.WriteLine);

                        // this REQUIRES a local
                        if (inspect.Where == null)
                            throw new Exception("Must include Where to inspect.");

                        var response = client.MakeRequest<StatusRequest, StatusResponse>(new StatusRequest()).GetAwaiter().GetResult();
                        Console.WriteLine("");
                        return 0;
                    },
                    errs => 1
                );

            _ = Console.ReadLine();
        }
    }
}

