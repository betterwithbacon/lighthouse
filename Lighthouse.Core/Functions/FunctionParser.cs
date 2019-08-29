using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Lighthouse.Core.Functions
{
    public class FunctionParser : IFunctionParser, IParameterizedFunctionParser, IOutputFunctionParser
    {
        public bool TryParse(string functionString, out Function function)
        {
            function = null;

            try
            {
                function = new Function(CSharpScript.Create(functionString));                
            }
            catch (Exception)
            {
                // swallow all errors, and just return null
            }

            return false;
        }

        public bool TryParse<T>(string functionString, out Function<T> function)
        {
            function = null;

            try
            {
                function = new Function<T>(CSharpScript.Create<T>(functionString));
            }
            catch (Exception)
            {
                // swallow all errors, and just return null
            }

            return false;
        }

        public bool TryParse<TInput, TOutput>(string functionString, out Function<TInput, TOutput> function)
        {
            function = null;

            try
            {
                function = new Function<TInput, TOutput>(CSharpScript.Create<TOutput>(functionString));
            }
            catch (Exception)
            {
                // swallow all errors, and just return null
            }

            return false;
        }
    }
}
