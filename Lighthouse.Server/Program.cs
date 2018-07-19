using System;

namespace Lighthouse.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // the server needs to know what it's doing
            var server = new LighthouseServer(
                clientLogger: (message) => Console.WriteLine(message)    
            );

            LighthouseLauncher
                .BuildService<LighthouseServer>("Lighthouse Environment Monitoring");
                //..AddComponent("")

            // start the server
            server.Start();

        }
    }
}
