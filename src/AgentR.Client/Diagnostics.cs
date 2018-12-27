using System.Diagnostics;

namespace AgentR.Client
{
    public static class Diagnostics
    {
        public static readonly TraceSource Tracer = new TraceSource("AgentR.Client");
    }
}
