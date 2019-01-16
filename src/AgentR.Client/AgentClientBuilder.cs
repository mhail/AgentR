using System;
using Microsoft.AspNetCore.SignalR.Client;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using AgentR.Client.SignalR;
using Microsoft.Extensions.Logging;

namespace AgentR.Client
{
    public interface IAgentClientBuilder
    {
        IHubConnectionBuilder Connection { get; }
        void HandleRequest<TRequest, TResponse>() where TRequest : IRequest<TResponse>;
        void ReconnectIn(Func<int, TimeSpan> callback);
    }

    public static class ServiceCollectionExtensions
    {
        public static void AddAgentR(this IServiceCollection services, Action<IAgentClientBuilder> configureAgent)
        {
            var builder = new AgentClientBuilder();
            configureAgent(builder);

            services.AddSingleton<IAgentClient>(p => new AgentClient(builder.GetHubConnection(p), p.GetService<IMediator>()));
        }
    }

    public class AgentClientBuilder : IAgentClientBuilder
    {
        private readonly HubConnectionBuilder connectionBuilder;
        private Action<HubConnection, IServiceProvider> OnConnectionCreated;

        public AgentClientBuilder()
        {
            this.connectionBuilder = new HubConnectionBuilder();
            OnConnectionCreated = (c, p) => { };
        }

        public IHubConnectionBuilder Connection => this.connectionBuilder;

        public void HandleRequest<TRequest, TResponse>() where TRequest : IRequest<TResponse>
        {
            OnConnectionCreated += AttachHandleRequestToConnection;

            void AttachHandleRequestToConnection(HubConnection cxn, IServiceProvider serviceProvider) => cxn.HandleRequest<TRequest, TResponse>(serviceProvider.GetService<IMediator>());
        }

        public HubConnection GetHubConnection(IServiceProvider serviceProvider)
        {
            var connection = connectionBuilder.Build();

            OnConnectionCreated(connection, serviceProvider);

            return connection;
        }

        public void ReconnectIn(Func<int, TimeSpan> callback)
        {
            OnConnectionCreated += AttachHandleReconnectInConnection;

            void AttachHandleReconnectInConnection(HubConnection cxn, IServiceProvider serviceProvider) 
            {
                var logger = serviceProvider.GetService<ILogger<AgentClientBuilder>>() ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<AgentClientBuilder>.Instance;
                cxn.Closed += async (error) =>
                {
                    logger.LogDebug("Disconnected");

                    int i = 0;
                    while (cxn.State != HubConnectionState.Connected)
                    {
                        await Task.Delay(callback(i));

                        logger.LogDebug("Reconnecting");
                        try
                        {
                            await cxn.StartAsync();
                        } catch (Exception ex)
                        {
                            logger.LogCritical(ex, "Reconnect Failed");
                        }
                        i++;
                    }
                };
            }

        }
    }
}
