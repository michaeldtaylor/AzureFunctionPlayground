using System.Diagnostics;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctionPlayground.Tests
{
    public class TestTraceWriter : TraceWriter
    {
        public TestTraceWriter(TraceLevel level) : base(level)
        {
        }

        public override void Trace(TraceEvent traceEvent)
        {
        }
    }
}