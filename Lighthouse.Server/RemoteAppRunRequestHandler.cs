using System;
using Lighthouse.Core;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.Utils;

namespace Lighthouse.Server
{
    public class RemoteAppRunRequestHandler : LighthouseServiceBase, IRequestHandler<RemoteAppRunRequest, RemoteAppRunHandle>
    {
        public RemoteAppRunHandle Handle(RemoteAppRunRequest request)
        {

            var handle = new RemoteAppRunHandle();

            try
            {

                // find the app to run
                Type appType = LighthouseFetcher.Fetch(request.What);


                // if not, also let them know
                if (appType == null)
                {
                    handle.Status = $"Can't find app with name: {request.What}";
                    return handle;
                }

                // if you can run it!
                // inform the container the run the app dats it!
                Container.Launch(appType, request.How).GetAwaiter().GetResult();
            }
            catch(Exception e)
            {
                handle.Status = e.Message; 
            }

            return handle;
        }
    }
}