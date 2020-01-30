using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lighthouse.Core.Events;
using Lighthouse.Core.Logging;
using Lighthouse.Core.Storage;

namespace Lighthouse.Core.Tests
{
    public class MockLighthouseServer : ILighthouseServiceContainer
    {
        public IWarehouse Warehouse => throw new NotImplementedException();

        private Func<object, object> OnHandleHandler { get; set; }

        public void AddLogger(Action<string> logger)
        {
            throw new NotImplementedException();
        }

        public Task AddScheduledAction(ILighthouseService owner, Action<DateTime> taskToPerform, int minuteFrequency = 1, string scheduleName = null)
        {
            throw new NotImplementedException();
        }

        internal void OnHandleRequest<TRequest, TResponse>(Func<object, object> handler)
        {
            OnHandleHandler = handler;
        }

        public void Bind(int port)
        {
        }

        public Task Do(Action<ILighthouseServiceContainer> action, string logMessage = "")
        {
            throw new NotImplementedException();
        }

        public Task EmitEvent(IEvent ev, object source = null)
        {
            throw new NotImplementedException();
        }

        public DateTime GetNow()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IResourceProvider> GetResourceProviders()
        {
            throw new NotImplementedException();
        }

        public async Task<TResponse> HandleRequest<TRequest, TResponse>(TRequest request) where TRequest : class
        {
            if (OnHandleHandler == null)
            {
                return await Task.FromResult(default(TResponse));
            }
            return await Task.FromResult((TResponse)OnHandleHandler(request));
        }

        public Task Launch(ILighthouseService service)
        {
            throw new NotImplementedException();
        }

        public void Log(LogLevel level, LogType logType, object sender, string message = null, Exception exception = null, bool emitEvent = true)
        {
            throw new NotImplementedException();
        }

        public void RegisterEventConsumer<TEvent>(IEventConsumer eventConsumer) where TEvent : IEvent
        {
            throw new NotImplementedException();
        }

        public void RegisterEventProducer(IEventProducer eventSource)
        {
            throw new NotImplementedException();
        }

        public void Register(ILighthousePeer peer, Dictionary<string, string> otherConfig = null)
        {
            // no op, it doesn't care here either
        }

        public Task RemoveScheduledActions(ILighthouseService owner, string scheduleName = null)
        {
            throw new NotImplementedException();
        }

        public T ResolveType<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public void RegisterResource(IResourceProvider resourceProvider)
        {
            throw new NotImplementedException();
        }

        public Task Launch(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
