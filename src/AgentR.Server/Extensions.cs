using System.Threading;
using System.Threading.Tasks;
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
  }
}