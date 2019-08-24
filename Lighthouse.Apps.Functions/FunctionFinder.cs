using Lighthouse.Core;
using Lighthouse.Core.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Apps.Functions
{
    public class FunctionFinder : IFunctionFinder
    {
        public  IEnumerable<Function> GetFunctions(ILighthouseServiceContainer container, string functionNamespace)
        {
            var functions = new List<Function>();
            var functionParser = container.ResolveType<IFunctionParser>();
            if (functionParser == null)
            {
                container.Log(Core.Logging.LogLevel.Debug, Core.Logging.LogType.Error, this, message: "IFunctionParser can't be loaded.");
                return functions;
            }

            var functionStrings = container
                .Warehouse
                .Retrieve<IList<string>>(StorageScope.Global, functionNamespace);

            if (functionStrings == null || functionStrings.Count == 0)
            {
                // no functions available
                container.Log(Core.Logging.LogLevel.Debug, Core.Logging.LogType.Info, this, message: "No functions to load");
                return functions;
            }
            
            foreach (var functionString in functionStrings)
            {
                if (functionParser.TryParse(functionString, out var function))
                {
                    functions.Add(function);
                }
                else
                {
                    container.Log(
                        Core.Logging.LogLevel.Debug,
                        Core.Logging.LogType.Error,
                        this,
                        message: $"Can't parse function string: {functionString.Substring(0, 100)}"
                    );
                }
            }

            return functions;
        }
    }

    public interface IFunctionFinder
    {
    }
}
