using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;

using MediatR;

using Server.Requests;

using AgentR;
using System.Diagnostics;

namespace Client {
  // This is the entry point for the client sample
  public class  MainHandler : IRequestHandler<MainRequest> {

    public static readonly TraceListener ConsoleListner = new TextWriterTraceListener(Console.OpenStandardOutput(), "console")
    {
        Filter = new System.Diagnostics.EventTypeFilter(System.Diagnostics.SourceLevels.All)
    };

    private readonly IMediator mediator;

    public MainHandler(IMediator mediator)
    {
        this.mediator = mediator;
    }
    public async Task<Unit> Handle(MainRequest request, CancellationToken cancellationToken)
        {
            AgentR.Diagnostics.Tracer.Listeners.Add(ConsoleListner);
            AgentR.Diagnostics.Tracer.Switch.Level = SourceLevels.All;

            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/agentr")
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            // Inform the AgentR hub that we can handle the following request types
            connection.HandleRequest<SampleRequest, Unit>(mediator);
            connection.HandleRequest<SampleRequest2, Unit>(mediator);

            await connection.StartAsync();

            // Wait for any requests
            do {
              await Task.Delay(1000);
            } while (Console.Read() <= 0);

            await connection.StopAsync();

            ConsoleListner.Flush();
            return Unit.Value;
        }
  }
}