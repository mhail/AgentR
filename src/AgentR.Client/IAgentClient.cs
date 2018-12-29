using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AgentR.Client.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace AgentR.Client
{
    public interface IAgentClient
    {
        Task StartAsync();
        Task StopAsync();
        void HandleRequest<TRequest, TResponse>() where TRequest : IRequest<TResponse>;
        Task<TResponse> SendRequest<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken));
    }

    public class AgentClient : IAgentClient
    {
        private readonly HubConnection connection;
        private readonly IMediator mediator;

        public AgentClient(HubConnection connection, IMediator mediator)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(HubConnection));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(IMediator));
        }

        public void HandleRequest<TRequest, TResponse>() where TRequest : IRequest<TResponse> => connection.HandleRequest<TRequest, TResponse>(this.mediator);

        public Task<TResponse> SendRequest<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return this.connection.SendRequest<TResponse>(request, cancellationToken);
        }

        public Task StartAsync() => connection.StartAsync();

        public Task StopAsync() => connection.StopAsync();
    }
}
