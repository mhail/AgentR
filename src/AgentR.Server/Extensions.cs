using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AgentR.Server
{
    internal static class Extensions
    {

        public static IClientProxy GetClientsForRequest<TRequest, TResponse>(this IHubContext<AgentHub> hub)
        {
            var group = AgentHub.GetGroupName(typeof(TRequest), typeof(TResponse));
            var client = hub.Clients.Group(group);
            return client;
        }


        public static async Task SendRequest<TRequest, TResponse>(this IClientProxy client, int callbackId, TRequest request, CancellationToken cancellationToken)
        {
            var agentMethod = AgentHub.GetAgentMethodName(typeof(TRequest), typeof(TResponse));
            await client.SendAsync(agentMethod, callbackId, request, cancellationToken);
        }

        public static async Task<object> Send(this IMediator mediator, Type responseType, object request)
        {
            mediator = mediator ?? throw new ArgumentNullException(nameof(IMediator));

            var send = typeof(IMediator).GetMethod(nameof(mediator.Send)).MakeGenericMethod(responseType);

            var task = send.Invoke(mediator, new[] { request, CancellationToken.None }) as Task;

            await Task.WhenAll(task);

            var result = task.GetType().GetProperty(nameof(Task<object>.Result)).GetValue(task);

            return result;
        }
    }
}