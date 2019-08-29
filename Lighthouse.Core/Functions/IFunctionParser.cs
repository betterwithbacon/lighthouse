using System.Threading.Tasks;

namespace Lighthouse.Core.Functions
{
    public interface IFunctionParser
    {
        bool TryParse(string functionString, out Function function);

    }

    public interface IOutputFunctionParser
    {
        bool TryParse<T>(string functionString, out Function<T> function);
    }

    public interface IParameterizedFunctionParser
    {
        bool TryParse<TInput, TOutput>(string functionString, out Function<TInput, TOutput> function);
    }
}