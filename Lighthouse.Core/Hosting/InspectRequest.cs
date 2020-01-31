using System.Collections.Generic;

namespace Lighthouse.Core.Hosting
{
    public class InspectRequest
    {
        public string What { get; set; }

        public override string ToString() => $"Inspect Request for {What}";
    }

    public class InspectResponse
    {
        public IList<string> RawResponse { get; set; } = new List<string>();
        public bool Exists { get; set; } = false;

        public override string ToString() => $"Inspect Response ({RawResponse.Count} lines)";
    }
}
