namespace Lighthouse.Apps.Functions
{
    public interface IFunctionParser
    {
        bool TryParse(string functionString, out Function function);
    }
}