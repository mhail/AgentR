using System.Diagnostics;

namespace AgentR
{
    public static class Diagnostics
    {
        public static readonly TraceSource Tracer = new TraceSource("AgentR.Client");
    }
}
