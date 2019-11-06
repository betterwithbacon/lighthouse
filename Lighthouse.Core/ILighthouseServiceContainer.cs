using Lighthouse.Core.Events;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using Lighthouse.Core.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lighthouse.Core
{
    public interface ILighthouseServiceContainer
	{
        void Start();

        Task Stop();

        string ServerName { get; }

        string WorkingDirectory { get; }

        void Log(LogLevel level, LogType logType, object sender, string message = null, Exception exception = null, bool emitEvent = true);

        Task Launch(ILighthouseService service);

        IWarehouse Warehouse { get; }

        DateTime GetNow();

        IEnumerable<INetworkProvider> GetNetworkProviders();

        IEnumerable<IFileSystemProvider> GetFileSystemProviders();
        
		void RegisterResourceProvider(IResourceProvider resourceProvider);

        Task Do(Action<ILighthouseServiceContainer> action, string logMessage = "");

        Task AddScheduledAction(ILighthouseService owner, Action<DateTime> taskToPerform, int minuteFrequency = 1, string scheduleName = null);

        Task RemoveScheduledActions(ILighthouseService owner, string scheduleName = null);

        void RegisterEventProducer(IEventProducer eventSource);

		void RegisterEventConsumer<TEvent>(IEventConsumer eventConsumer)
			where TEvent : IEvent;

		Task EmitEvent(IEvent ev, object source = null);

		IEnumerable<IEvent> GetAllReceivedEvents(PointInTime since = null);
		
        void RegisterRemotePeer(ILighthouseServiceContainerConnection connection);

        IEnumerable<ILighthouseServiceContainerConnection> GetServerConnections();

        ILighthouseServiceContainerConnection Connect(Uri uri);

        T ResolveType<T>() where T : class;

        TResponse HandleRequest<TRequest, TResponse>(TRequest storageRequest)
            where TRequest : class;
    }
}