using System.Diagnostics;

namespace AgentR.Hubs
{
    public static class Diagnostics
    {
        public static readonly TraceSource Tracer = new TraceSource("AgentR.Server");
    }
}