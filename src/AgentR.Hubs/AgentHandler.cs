using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AgentR.Hubs {
   public class AgentHandler<T, R> : IRequestHandler<T, R> where T : IRequest<R>
    {
        private readonly IHubContext<AgentHub> hub;

        public AgentHandler(IHubContext<AgentHub> hub)
        {
            this.hub = hub ?? throw new ArgumentNullException(nameof(IHubContext<AgentHub>));
        }

        public Task<R> Handle(T request, CancellationToken cancellationToken)
        {
            Console.WriteLine("AgentHandler+Handle");
            return AgentHub.Handle<T, R>(hub, request, cancellationToken);
        }
    }
}