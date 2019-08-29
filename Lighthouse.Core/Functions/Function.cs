using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;

namespace Lighthouse.Core.Functions
{
    public class Function
    {
        private Script<object> script;

        public Function(Script<object> script)
        {
            this.script = script;
        }

        public void Execute()
        {
        }
    }

    public class Function<T>
    {
        private Script<T> script;

        public Function(Script<T> script)
        {
            this.script = script;
        }

        public async Task<T> Execute()
        {
            if (script == null)
                return default;
            else
            {
                var val = await script.RunAsync();
                return val.ReturnValue;
            }
        }
    }

    public class Function<TInput, TOutput>
    {
        private Script<TOutput> script;

        public Function(Script<TOutput> script)
        {
            this.script = script;
        }

        public async Task<TOutput> Execute(TInput input)
        {
            if (script == null)
                return default;
            else
            {
                var val = await script.RunAsync();
                return val.ReturnValue;
            }
        }
    }

}
