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
            [Option("what", Required = false, HelpText = "What to do?")]
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

        [Verb("stop")]
        public class StopOptions : BaseLighthouseOptions
        {
        }


        [Verb("store")]
        public class StoreOptions : BaseLighthouseOptions
        {
        }

        [Verb("retrieve")]
        public class RetrieveOptions : BaseLighthouseOptions
        {
        }

        [Verb("configure")]
        public class ConfigureOptions : BaseLighthouseOptions
        {
        }
                
        static void Main(string[] args)
        {
            var runner = new CommandLineRunner(Console.WriteLine,Console.ReadLine);
            var result = runner.Run(args);

            _ = Console.ReadLine();
        }
    }
}

