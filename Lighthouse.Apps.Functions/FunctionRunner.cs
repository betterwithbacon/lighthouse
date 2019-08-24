using Lighthouse.Core;
using Lighthouse.Core.Storage;
using System;
using System.Collections.Generic;

namespace Lighthouse.Apps.Functions
{
    public class FunctionRunner : LighthouseServiceBase
    {
        public const string StorageKeyName = "FunctionRunner_Functions";

        public List<Function> Functions { get; private set; }

        protected override void OnStart()
        {
            var functionFinder = Container.ResolveType<IFunctionFinder>();
            if (functionFinder == null)
            {
                container.Log(Core.Logging.LogLevel.Debug, Core.Logging.LogType.Error, this, message: "IFunctionParser can't be loaded.");
                return;
            }


        }
    }
}
