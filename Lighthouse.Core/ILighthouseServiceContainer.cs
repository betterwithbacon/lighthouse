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
        Task Launch(Type serviceType, object launchContext = null);

        Task Launch(ILighthouseService service, object launchContext = null);
        
        Warehouse Warehouse { get; }

        IEnumerable<IResourceProvider> GetResourceProviders();

        IEnumerable<ILighthouseService> GetRunningServices();

        void RegisterResource(IResourceProvider resourceProvider);

        Task Do(Action<ILighthouseServiceContainer> action, string logMessage = "");

        Task AddScheduledAction(ILighthouseService owner, Action<DateTime> taskToPerform, int minuteFrequency = 1, string scheduleName = null);

        Task RemoveScheduledActions(ILighthouseService owner, string scheduleName = null);

        void RegisterEventProducer(IEventProducer eventSource);

        void RegisterEventConsumer<TEvent>(IEventConsumer eventConsumer)
			where TEvent : IEvent;

		Task EmitEvent(IEvent ev, object source = null);

        T ResolveType<T>() where T : class;

        void Bind(int port);

        IEnumerable<ILighthousePeer> GetPeers();
    }

    public static class ContainerExtensions
    {
        public static IEnumerable<T> GetResourceProviders<T>(this ILighthouseServiceContainer container)
            where T : IResourceProvider => container.GetResourceProviders().OfType<T>();

        public static IEnumerable<INetworkProvider> GetNetworkProviders(this ILighthouseServiceContainer container) => container.GetResourceProviders<INetworkProvider>();

        public static IEnumerable<IFileSystemProvider> GetFileSystemProviders(this ILighthouseServiceContainer container) => container.GetResourceProviders<IFileSystemProvider>();
    }
}