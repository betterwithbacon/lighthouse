using Lighthouse.Core;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Functions;
using Lighthouse.Core.Storage;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Lighthouse.Apps.Functions
{
    public class FunctionRunner : LighthouseServiceBase
    {
        public const string StorageKeyName = "FunctionRunner_Functions";

        public IReadOnlyList<Function> Functions { get; private set; }

        protected override async Task OnStart()
        {
            var functionFinder = Container.ResolveType<IFunctionFinder>();
            if (functionFinder == null)
            {
                Container.Log(Core.Logging.LogLevel.Debug, Core.Logging.LogType.Error, this, message: "IFunctionParser can't be loaded.");
                throw new Exception("IFunctionParser can't be loaded.");
            }

            var functions = functionFinder.GetFunctions(Container, StorageKeyName);
            if(functions == null || !functions.Any())
            {
                Container.Log(Core.Logging.LogLevel.Debug, Core.Logging.LogType.Error, this, message: "No functions found.");
            }

            Functions = functions.ToImmutableList();

            await Task.CompletedTask;
        }
    }

    [ExternalLighthouseService("function")]
    public class FunctionExecuter : LighthouseServiceBase
    {

    }
}
