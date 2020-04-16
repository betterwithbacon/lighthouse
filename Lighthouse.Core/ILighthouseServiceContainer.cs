using Lighthouse.Core.Events;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using Lighthouse.Core.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lighthouse.Core
{
    /// <summary>
    /// An object capable of speaking with a container viable the Request API
    /// </summary>
    public interface ILighthouseClient
    {
        Task<TResponse> MakeRequest<TRequest, TResponse>(TRequest request)
            where TRequest : class;
    }

    public interface ILighthouseEnvironment
    {
        void AddLogger(Action<string> logger);

        void Log(LogLevel level, LogType logType, object sender, string message = null, Exception exception = null, bool emitEvent = true);
        DateTime GetNow();
    }

    /// <summary>
    ///  A container that exposes internal state of the container
    /// </summary>
    public interface IPriviledgedLighthouseServiceContainer : ILighthouseServiceContainer
    {
        IProducerConsumerCollection<IResourceProvider> Resources { get; }
        Stack<Action<string>> Loggers { get; }
    }

    /// <summary>
    /// Provides: scheduling, event bus, request/response infrastructure
    /// </summary>
    public interface ILighthouseServiceContainer : ILighthouseEnvironment
    {
        Task Launch(Type serviceType, object launchContext = null);

        Task Launch(ILighthouseService service, object launchContext = null);

        IEnumerable<IResourceProvider> GetResourceProviders();

        IEnumerable<ILighthouseService> GetRunningServices();

        Warehouse Warehouse { get; }
        
        void RegisterResource(IResourceProvider resourceProvider);

        Task Do(Action<ILighthouseServiceContainer> action, string logMessage = "");

        Task AddScheduledAction(ILighthouseService owner, Action<DateTime> taskToPerform, int minuteFrequency = 1, string scheduleName = null);

        Task RemoveScheduledActions(ILighthouseService owner, string scheduleName = null);

        void RegisterEventProducer(IEventProducer eventSource);
        
        void RegisterEventConsumer<TEvent>(IEventConsumer<TEvent> eventConsumer)
            where TEvent : IEvent;

		Task EmitEvent(IEvent ev, object source = null);

        T ResolveType<T>() where T : class;

        void Bind(int port);

        void RunPriveleged(ILighthouseService source, Action<IPriviledgedLighthouseServiceContainer> act);

        Task<TResponse> HandleRequest<TRequest, TResponse>(TRequest request)
            where TRequest : class;

        Task<TResponse> MakeRequest<TRequest, TResponse>(string serverName, TRequest request)
            where TRequest : class;
    }

    public static class ContainerExtensions
    {
        public static IEnumerable<T> GetResourceProviders<T>(this ILighthouseServiceContainer container)
            where T : IResourceProvider => container.GetResourceProviders().OfType<T>();

        public static INetworkProvider GetNetworkProvider(this ILighthouseServiceContainer container) => container.GetResourceProviders<INetworkProvider>().FirstOrDefault();

        /// <summary>
        /// Returns a list of file system providers available on the target server. A given file system could support multiple drives
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IFileSystemProvider GetFileSystem(this ILighthouseServiceContainer container) => container.GetResourceProviders<IFileSystemProvider>().FirstOrDefault();
    }
}