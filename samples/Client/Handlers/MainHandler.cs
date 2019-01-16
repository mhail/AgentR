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
        private readonly ILogger logger;
        private readonly IMediator mediator;
        private readonly IAgentClient agentClient;

        public MainHandler(IMediator mediator, IAgentClient agentClient, ILogger<MainHandler> logger)
        {
            this.mediator = mediator;
            this.agentClient = agentClient;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Unit> Handle(MainRequest request, CancellationToken cancellationToken)
        {
            await agentClient.TryConnect();

            var info = await mediator.Send(new ServerInfoRequest());

            logger.LogInformation($"Server key is: '{info.Key}'");

            // Wait for any requests
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            } while (Console.Read() <= 0);

            await agentClient.StopAsync();

            logger.LogInformation("Exit");
            return Unit.Value;
        }
    }
}