using Lighthouse.Core.Events;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using Lighthouse.Core.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lighthouse.Core
{
    /// <summary>
    /// An object capable of speaking with a container viable the Request API
    /// </summary>
    public interface ILighthousePeer
    {
        Task<TResponse> HandleRequest<TRequest, TResponse>(TRequest request)
            where TRequest : class;
    }

    public interface ILighthouseEnvironment
    {
        void AddLogger(Action<string> logger);

        void Log(LogLevel level, LogType logType, object sender, string message = null, Exception exception = null, bool emitEvent = true);
        DateTime GetNow();
    }

    /// <summary>
    /// Provides: scheduling, event bus, request/response infrastructure
    /// </summary>
    public interface ILighthouseServiceContainer : ILighthousePeer, ILighthouseEnvironment
    {
        Task Launch(ILighthouseService service);
        
        // ? should this be manually added? or apart of the deal
        IWarehouse Warehouse { get; }

        IEnumerable<IResourceProvider> GetResourceProviders();

        void RegisterResourceProvider(IResourceProvider resourceProvider);

        Task Do(Action<ILighthouseServiceContainer> action, string logMessage = "");

        Task AddScheduledAction(ILighthouseService owner, Action<DateTime> taskToPerform, int minuteFrequency = 1, string scheduleName = null);

        Task RemoveScheduledActions(ILighthouseService owner, string scheduleName = null);

        void RegisterEventProducer(IEventProducer eventSource);

		void RegisterEventConsumer<TEvent>(IEventConsumer eventConsumer)
			where TEvent : IEvent;

		Task EmitEvent(IEvent ev, object source = null);

        Task<TResponse> HandleRequest<TRequest, TResponse>(TRequest request)
            where TRequest : class;

        T ResolveType<T>() where T : class;

        void Bind(int port);
    }

    public static class ContainerExtensions
    {
        public static IEnumerable<T> GetResourceProviders<T>(this ILighthouseServiceContainer container)
            where T : IResourceProvider => container.GetResourceProviders().OfType<T>();

        public static IEnumerable<INetworkProvider> GetNetworkProviders(this ILighthouseServiceContainer container)
        {
            yield return null;
        }

        public static IEnumerable<IFileSystemProvider> GetFileSystemProviders(this ILighthouseServiceContainer container)
        {
            yield return null;
        }
    }
}