using Lighthouse.Core;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lighthouse.Client
{
    public class LighthouseClient : ILighthousePeer, ILighthouseEnvironment
    {
        private List<Action<string>> Loggers { get; set; } = new List<Action<string>>();
        public Uri Uri { get; }
        public INetworkProvider NetworkProvider { get; }

        public LighthouseClient(Uri uri, INetworkProvider networkProvider = null)
        {
            Uri = uri;
            NetworkProvider = networkProvider ?? new InternetNetworkProvider();
        }

        public void AddLogger(Action<string> logger)
        {
            if (logger != null)
            {
                Loggers.Add(logger);
            }
        }

        public DateTime GetNow() => DateTime.Now;
        

        public void Log(LogLevel level, LogType logType, object sender, string message = null, Exception exception = null, bool emitEvent = true)
        {
            foreach(var logger in Loggers)
            {
                if (exception == null)
                {
                    logger($"{GetNow()}-{level}-{logType}-{sender}:{message}.");
                }
                else
                {
                    logger($"{GetNow()}-{level}-{logType}-{sender}:{message}. Exception: {exception}");
                }
            }
        }

        public async Task<TResponse> HandleRequest<TRequest, TResponse>(TRequest request) where TRequest : class =>
            // a VERY naive implementation, says that ALL messages are sent to a single port
            // , and that, we don't need endpoints because of that.
            await NetworkProvider.GetObjectAsync<TRequest, TResponse>(Uri, request, true);
    }
}
