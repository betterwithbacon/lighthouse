using System.Collections.Generic;

namespace Lighthouse.Core
{
    public class ResourceResponse
    {
        public IList<string> ActionsTaken { get; private set; } = new List<string>();
    }
}
