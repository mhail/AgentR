using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;

using MediatR;

using Server.Requests;

using AgentR.Client;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Client
{
    // This is the entry point for the client sample
    public class MainHandler : IRequestHandler<MainRequest>
    {

        public static readonly TraceListener ConsoleListner = new TextWriterTraceListener(Console.OpenStandardOutput(), "console")
        {
            Filter = new System.Diagnostics.EventTypeFilter(System.Diagnostics.SourceLevels.All)
        };

        private readonly IMediator mediator;
        private readonly IAgentClient agentClient;

        public MainHandler(IMediator mediator, IAgentClient agentClient)
        {
            this.mediator = mediator;
            this.agentClient = agentClient;
        }
        public async Task<Unit> Handle(MainRequest request, CancellationToken cancellationToken)
        {
            AgentR.Client.Diagnostics.Tracer.Listeners.Add(ConsoleListner);
            AgentR.Client.Diagnostics.Tracer.Switch.Level = SourceLevels.All;


            await agentClient.StartAsync();

            var info = await mediator.Send(new ServerInfoRequest());

            Console.WriteLine($"Server key is: '{info.Key}'");

            // Wait for any requests
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            } while (Console.Read() <= 0);

            await agentClient.StopAsync();

            ConsoleListner.Flush();
            return Unit.Value;
        }
    }
}