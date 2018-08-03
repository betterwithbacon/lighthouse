using System;

namespace Lighthouse.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");

            // the server needs to know what it's doing
            //var server = new LighthouseServer(
            //    clientLogger: (message) => Console.WriteLine(message)    
            //);

			//var server = LighthouseLauncher
			//	.BuildService<LighthouseServer>("Lighthouse Environment Monitoring")
			//	.AddClientLogger((message) => Console.WriteLine(message))
			//	.SetRuntimeEnvironment(RuntimeEnvironment.)

			// start the server
			//server.Start();


        }
    }
}
