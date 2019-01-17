using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTests")]

namespace AgentR.Server
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

            cancellationToken.Register(completion.SetCanceled);

            var clients = await GetClientsForRequest(request, cancellationToken);

            var callbackid = await storage.CreateCallback<TRequest, TResponse>(request, completion);

            Diagnostics.Tracer.TraceInformation($"sending request {callbackid}");

            await AwaitAccepted(request, clients, callbackid, cancellationToken);

            var result = await completion.Task;

            return result;
        }

        protected virtual async Task AwaitAccepted(TRequest request, IClientProxy clients, int callbackid, CancellationToken cancellationToken)
        {
            await clients.SendRequest<TRequest, TResponse>(callbackid, request, cancellationToken);

            bool accepted = false;
            int i = 0; 
            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                accepted = await storage.IsAccepted(callbackid);

                if (accepted) break;

                await WaitForClientsToAccept(i);

                cancellationToken.ThrowIfCancellationRequested();

                await clients.SendRequest<TRequest, TResponse>(callbackid, request, cancellationToken);
            } while (!accepted);
        }

        protected virtual async Task WaitForClientsToAccept(int i)
        {
            await Task.Delay(1 + ( 2 * i % 11));
        }

        /// <summary>
        /// Gets the clients for request, this is mrthod that can be overidden
        /// to allow for furthur filtering of the clients that can respond to 
        /// the request.
        /// </summary>
        /// <returns>The clients that can respond to the request</returns>
        /// <param name="request">Request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        protected virtual Task<IClientProxy> GetClientsForRequest(TRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult( hub.GetClientsForRequest<TRequest, TResponse>() );
        }

    }
}