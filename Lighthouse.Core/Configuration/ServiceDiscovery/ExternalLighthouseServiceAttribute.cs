using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Configuration.ServiceDiscovery
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ExternalLighthouseServiceAttribute : Attribute
    {
        // This is a positional argument
        public ExternalLighthouseServiceAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
    }
}
