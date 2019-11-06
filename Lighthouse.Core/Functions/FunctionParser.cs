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

            if (!string.IsNullOrEmpty(functionString))
            {
                try
                {
                    function = new Function(CSharpScript.Create(functionString));
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch (Exception)
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                {
                    // swallow all errors, and just return null
                }

                return false;
            }
            return false;
        }

        public bool TryParse<T>(string functionString, out Function<T> function)
        {
            function = null;

            if (!string.IsNullOrEmpty(functionString))
            {
                try
                {
                    function = new Function<T>(CSharpScript.Create<T>(functionString));
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                {
                    // swallow all errors, and just return null
                }
            }
            return false;
        }

        public bool TryParse<TInput, TOutput>(string functionString, out Function<TInput, TOutput> function)
        {
            function = null;

            try
            {
                function = new Function<TInput, TOutput>(
                    CSharpScript.Create<TOutput>(
                        functionString,
                        globalsType: typeof(TInput))
                    );
            }
            catch (Exception)
            {
                // swallow all errors, and just return null
            }

            return false;
        }
    }
}
