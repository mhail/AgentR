using System.Diagnostics;

namespace AgentR.Server
{
    public static class Diagnostics
    {
        public static readonly TraceSource Tracer = new TraceSource("AgentR.Server");
    }
}