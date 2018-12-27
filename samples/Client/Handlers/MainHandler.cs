using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;

using MediatR;

using Server.Requests;

using AgentR;

namespace Client {
  // This is the entry point for the client sample
  public class  MainHandler : IRequestHandler<MainRequest> {

    private readonly IMediator mediator;
    public MainHandler(IMediator mediator)
    {
        this.mediator = mediator;
    }
    public async Task<Unit> Handle(MainRequest request, CancellationToken cancellationToken)
        {

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

            return Unit.Value;
        }
  }
}