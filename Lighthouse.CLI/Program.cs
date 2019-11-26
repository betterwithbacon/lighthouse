using CommandLine;
using Lighthouse.Client;
using Lighthouse.Core;
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
        }

        [Verb("inspect")]
        public class ViewOptions : BaseLighthouseOptions
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
            var client = new LighthouseClient();
            client.AddLogger((message) => console.WriteLine(message));

            Parser.Default.ParseArguments<RunOptions, ViewOptions>(args)
                .MapResult(
                    (RunOptions runOptions) =>
                    {
                        if (string.IsNullOrEmpty(runOptions.Application) && string.IsNullOrEmpty(runOptions.File))
                        {
                            throw new ApplicationException("Can't find app name");
                        }

                        if (!runOptions.IsAppMode)
                        {
                            var fileContents = File.ReadAllText(runOptions.File);

                            var config = YamlUtil.ParseYaml<LighthouseRunConfig>(fileContents);

                            //(IEnumerable<ResourceProviderConfig> Resources, IEnumerable<Type> Types) = YamlV1Decomposer.Deserialize(fileContents);

                            //var failedResourceCreations = new List<string>();

                            //foreach (var resourceConfig in Resources)
                            //{
                            //    (bool wasSuccessful, string errorReason) = ResourceFactory.TryCreate(resourceConfig, out var resource);

                            //    if (wasSuccessful)
                            //    {
                            //        server.RegisterResourceProvider(resource);
                            //    }
                            //    else
                            //    {
                            //        failedResourceCreations.Add(errorReason);
                            //    }
                            //}

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
                            Type appType = LighthouseFetcher.Fetch(runOptions.Application);
                            if (appType == null)
                            {
                                throw new ApplicationException("Can't find app name");
                            }

                            server.Launch(appType);
                        }

                        return 0;
                    },
                    (ViewOptions viewOptions) =>
                    {
                        if (Uri.TryCreate(viewOptions.Server, UriKind.Absolute, out var uri))
                        {
                            //var connection = server.Connect(uri);

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

