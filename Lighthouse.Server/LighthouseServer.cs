using Lighthouse.Core;
using System;

namespace Lighthouse.Server
{
    public class LighthouseServer : LighthouseServiceBase
    {        
        public LighthouseServer(Action<string> clientLogger)
        {
            // first get local logging sorted out
            // the client which creates the server, should manage how local logging should be handled
            // the logging doesn't need to be exhaustive, because the services themselves might implement logging
            InitializeClientLogger(clientLogger);
        }
    }
}