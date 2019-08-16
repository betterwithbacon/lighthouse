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
            var functionParser = Container.ResolveType<IFunctionParser>();
            if (functionParser == null)
            {
                Container.Log(Core.Logging.LogLevel.Debug, Core.Logging.LogType.Error, this, message: "IFunctionParser can't be loaded.");
                return;
            }

            var functions = Container
                .Warehouse
                .Retrieve<IList<string>>(StorageScope.Global, StorageKeyName);

            if (functions == null || functions.Count == 0)
            {
                // no functions available
                Container.Log(Core.Logging.LogLevel.Debug, Core.Logging.LogType.Info, this, message: "No functions to load");
                return;
            }

            Functions = new List<Function>();

            foreach (var functionString in functions)
            {
                if(functionParser.TryParse(functionString, out var function))
                {
                    Functions.Add(function);
                }
                else
                {
                    Container.Log(
                        Core.Logging.LogLevel.Debug, 
                        Core.Logging.LogType.Error, 
                        this, 
                        message: $"Can't parse function string: {functionString.Substring(0,100)}"
                    );
                }
            }
        }
    }
}
