using System;
using System.Collections.Generic;

namespace Lighthouse.Core.Hosting
{
    public interface ILighthouseServiceHasState
    {
        IEnumerable<string> GetState();
    }
}
