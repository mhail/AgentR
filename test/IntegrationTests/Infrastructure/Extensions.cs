using System;
using System.Threading;
using System.Threading.Tasks;
using AgentR.Server;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace IntegrationTests
{
    public static class Extensions
    {
        public static IRequestHandler<TRequest, TResponse> CreateHandler<TRequest, TResponse>(this IHubContext<AgentHub> hub)
            where TRequest : IRequest<TResponse>
        {
            if (null == hub) throw new ArgumentNullException(nameof(hub));

            return new AgentHandler<TRequest, TResponse>(hub);
        }

        public static Task<TResponse> SendRequest<TResponse>(this IHubContext<AgentHub> hub, IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = typeof(Extensions).GetMethod(nameof(CreateHandler)).MakeGenericMethod(request.GetType(), typeof(TResponse));

            var handler = method.Invoke(null, new[] { hub });

            var handle = handler.GetType().GetMethod(nameof(IRequestHandler<IRequest<Unit>>.Handle));

            return (Task<TResponse>)handle.Invoke(handler, new object[] { request, cancellationToken });
        }

    }
}