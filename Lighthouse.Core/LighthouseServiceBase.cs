using System;
using System.Collections.Generic;

namespace Lighthouse.Core
{
    public abstract class LighthouseServiceBase : ILighthouseService
        //<TConfig> : ILighthouseService<TConfig>
        //where TConfig : ILighthouseServiceConfigurationContext, new()
    {
        List<ILighthouseServiceComponent> Components { get; set; }
        private ILighthouseServiceConfigurationContext ConfigurationContext { get; set; }
        protected Action<string> ClientLogger { get; set; }


        protected virtual void OnServiceStartBegin()
        { }

        protected virtual void OnServiceStartEnd()
        { }

        protected virtual void OnThreadStartBegin()
        { }

        protected virtual void OnThreadStartEnd()
        { }

        protected virtual void OnLoadComponent(ILighthouseServiceComponent component)
        { }

        public void Start()
        {
            Initialize();

            OnServiceStartBegin();

            StartThreads();

            OnServiceStartEnd();
        }

        private void StartThreads()
        {
            
        }

        private void Initialize()
        {
            LoadConfiguration();

            Components.ForEach(LoadComponent);
        }

        protected virtual void LoadConfiguration()
        {
            ConfigurationContext = new StandardServiceConfig();
        }

        private void LoadComponent(ILighthouseServiceComponent component)
        {
            OnLoadComponent(component);
        }

        protected void InitializeClientLogger(Action<string> clientLogger)
        {
            ClientLogger = clientLogger;
        }
    }
}
