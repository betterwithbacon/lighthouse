using System;
using System.Collections.Generic;
using System.Linq;
using Lighthouse.Core;
using Lighthouse.Core.Events;
using Lighthouse.Core.Logging;

namespace Lighthouse.Server
{
    public class ResourceManager : LighthouseServiceBase, IRequestHandler<ResourceRequest, ResourceResponse>, IEventConsumer
    {
        public ResourceResponse Handle(ResourceRequest request)
        {
            var response = new ResourceResponse();

            // first locate the resource it's talking about
            switch (request.RequestType)
            {
                case ResourceRequestType.Add:
                    // find the type of the resource they want to add
                    var (wasSuccessful, errors) = ResourceFactory.TryCreate(request.Config, out var resourceProvider);

                    if(wasSuccessful)
                    {
                        // create the resource now add it
                        Register(resourceProvider);
                        response.ActionsTaken.Add("added");
                    }
                    else
                    {
                        response.ActionsTaken.Add("failed");
                        throw new Exception(errors);                        
                    }
                    break;
                case ResourceRequestType.Remove:

                    break;
                case ResourceRequestType.Inspect:
                    break;
            }
            return response;
        }

        public void HandleEvent(IEvent ev)
        {
            throw new NotImplementedException();
        }

        //public T Find<T>(string name)
        //    where T : IResourceProvider
        //{
        //    T resource = default;

        //    Container.RunPriveleged(
        //        this,
        //        container => container.Resources.OfType<T>().Where(r => r.ExternalServiceName()
        //    );

        //    return resource;
        //}

        public void Register(IResourceProvider resourceProvider)
        {
            Container.Log(LogLevel.Debug, LogType.Info, this, $"Added resource: {resourceProvider}.");

            // inform the resource of what is reigstering it
            resourceProvider.Register(Container);

            // it an escalated mode, add the resource to the container
            Container.RunPriveleged(
                this,
                container => container.Resources.TryAdd(resourceProvider)
            );

            // inform the container that the resource is available
            Container.EmitEvent(new ResourceAvailableEvent(Container, resourceProvider));
        }
    }
}