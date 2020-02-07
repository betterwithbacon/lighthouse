using System;
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

        public override string ToString() => string.Join(Environment.NewLine, RawResponse);
    }

    public class StopRequest
    {
        public string What { get; set; }

        public override string ToString() => $"Stop Request for {What}";
    }
}
