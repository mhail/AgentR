using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AgentR.Hubs
{
    public class AgentHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IHubContext<AgentHub> hub;
        private readonly IRequestCallbackCordinator storage;

        public AgentHandler(IHubContext<AgentHub> hub) 
            : this(hub, InMemoryCallbackCordinator.Instance)
        {
        }

        internal AgentHandler(IHubContext<AgentHub> hub, IRequestCallbackCordinator storage)
        {
            this.hub = hub ?? throw new ArgumentNullException(nameof(IHubContext<AgentHub>));
            this.storage = storage ?? throw new ArgumentNullException(nameof(IRequestCallbackCordinator));
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            Diagnostics.Tracer.TraceInformation("AgentHandler+Handle");

            var completion = new TaskCompletionSource<TResponse>();

            var callbackid = await storage.CreateCallback(completion);

            var clients = hub.GetClientsForRequest<TRequest, TResponse>();

            Diagnostics.Tracer.TraceInformation($"sending request {callbackid}");

            await clients.SendRequest<TRequest, TResponse>(callbackid, request, cancellationToken);

            var result = await completion.Task;

            return result;
        }
  }
}