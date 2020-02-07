using System;
using System.Collections.Generic;
using System.Linq;
using Lighthouse.Core;
using Lighthouse.Core.Hosting;

namespace Lighthouse.Server
{
    public class InspectHandler : LighthouseServiceBase, IRequestHandler<InspectRequest,InspectResponse>
    {
        public InspectHandler()
        {
        }

        public InspectResponse Handle(InspectRequest request)
        {
            // we're curious about something in the container
            var service = Container.GetRunningServices().FirstOrDefault(s => s.Id.Equals(request.What, StringComparison.OrdinalIgnoreCase));

            if(service == null)
            {
                // can't find service...do no more
                return new InspectResponse
                {
                    Exists = false
                };
            }
            else
            {
                List<string> state = new List<string>();

                // we found it so just check to see if the service is inspectable..if not, then just dump ToString()
                if (service is ILighthouseServiceHasState statefulService)
                {
                    state.AddRange(statefulService.GetState());
                }
                else
                {
                    state.Add(service.ToString());
                }

                return new InspectResponse
                {
                    RawResponse = state,
                    Exists = true
                };
            }

        }
    }
}
