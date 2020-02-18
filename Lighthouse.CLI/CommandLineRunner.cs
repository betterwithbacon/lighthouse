using System;
using System.IO;
using CommandLine;
using Lighthouse.Client;
using Lighthouse.Core;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.Utils;
using Lighthouse.Server;
using static Lighthouse.CLI.Program;
using System.Linq;
using System.Collections.Generic;
using Lighthouse.Core.IO;
using Lighthouse.Core.Storage;

namespace Lighthouse.CLI
{
    public class CommandLineRunner
    {
        public CommandLineRunner(Action<string> consoleWrite, Func<string> consoleRead,
            TypeFactory typeFactory = null)
        { 
            ConsoleWrite = consoleWrite;
            ConsoleRead = consoleRead;
            if(typeFactory== null)
            {
                typeFactory = new TypeFactory();
                typeFactory.Register<INetworkProvider>(() => new InternetNetworkProvider());
            }
            else
            {
                TypeFactory = typeFactory;
            }            
        }

        public Action<string> ConsoleWrite { get; }
        public Func<string> ConsoleRead { get; }
        public TypeFactory TypeFactory { get; }
        
        public int Run(IEnumerable<string> args)
        {
            LighthouseClient GetClient(Uri target)
            {
                var networkProvider = TypeFactory.Create<INetworkProvider>();
                var client = new LighthouseClient(target, networkProvider);
                client.AddLogger(ConsoleWrite);
                return client;
            }

            var result = Parser.Default.ParseArguments<RunOptions, InspectOptions, StopOptions, StoreOptions, RetrieveOptions>(args)
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
                        if (run.Where != null)
                        {
                            var client = GetClient(run.Where.ToUri());

                            // make a connection to the other server
                            var response = client.HandleRequest<RemoteAppRunRequest, RemoteAppRunHandle>(new RemoteAppRunRequest(run.What)).GetAwaiter().GetResult();
                            ConsoleWrite($"Request {response?.Status ?? "no response"} (ID: {response?.Id ?? "no ID"})");
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
                            server.AddLogger(ConsoleWrite);
                            server.Launch(appType).GetAwaiter().GetResult();
                        }
                    }

                    return 0;
                },
                (InspectOptions inspect) =>
                {
                    var client = GetClient(inspect.Where.ToUri());

                    if (inspect.Where == null)
                        throw new Exception("Must include Where to inspect.");

                    if (inspect.What == null)
                    {
                        var response = client.HandleRequest<StatusRequest, StatusResponse>(new StatusRequest()).GetAwaiter().GetResult();
                        ConsoleWrite(response.ToString());
                    }
                    else
                    {
                        var response = client.HandleRequest<InspectRequest, InspectResponse>(
                            new InspectRequest { What = inspect.What }
                        ).GetAwaiter().GetResult();

                        ConsoleWrite(string.Join(Environment.NewLine, response.RawResponse));
                    }

                    return 0;
                },
                (StopOptions stop) =>
                {
                    if(stop.What == null || stop.Where == null)
                    {
                        throw new Exception("Stop what and where?");
                    }

                    var client = GetClient(stop.Where.ToUri());

                    var response = client.HandleRequest<StopRequest, bool>(
                            new StopRequest { What = stop.What }
                        ).GetAwaiter().GetResult();

                    ConsoleWrite(response ? $"{stop.What} stopped on {stop.Where}" : "failed");

                    return 0;
                },
                (StoreOptions store) =>
                {
                    if (store.What == null || store.Where == null)
                    {
                        throw new Exception("Stop what and where?");
                    }

                    var client = GetClient(store.Where.ToUri());
                    var deserialize = store.What.DeserializeFromJSON<WarehouseStoreRequest>();
                    
                    var response = client.HandleRequest<WarehouseStoreRequest, bool>(
                            new WarehouseStoreRequest { Key = deserialize.Key, Value=deserialize.Value }
                        ).GetAwaiter().GetResult();

                    ConsoleWrite(response ? "stored" : "failed");

                    return 0;
                },
                (RetrieveOptions retrieve) =>
                {
                    if (retrieve.What == null || retrieve.Where == null)
                    {
                        throw new Exception("Stop what and where?");
                    }

                    var client = GetClient(retrieve.Where.ToUri());
                    var deserialize = retrieve.What.DeserializeFromJSON<WarehouseRetrieveRequest>();

                    var response = client.HandleRequest<WarehouseRetrieveRequest, WarehouseRetrieveResponse>(
                            new WarehouseRetrieveRequest { Key = deserialize.Key}
                        ).GetAwaiter().GetResult();

                    ConsoleWrite(response.Value);

                    return 0;
                },
                errs =>
                {
                    foreach(var error in errs)
                    {
                        ConsoleWrite(error.ToString());
                    }

                    throw new Exception(string.Join(",", errs));
                    //return errs.Count();
                }); 

            return 0;
        }
    }
}
