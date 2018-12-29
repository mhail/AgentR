using System.Threading;
using System.Threading.Tasks;
using AgentR.Client;
using MediatR;
using Server.Requests;

namespace Client.Handlers
{
    public class ServerInfoRequestHandler : IRequestHandler<ServerInfoRequest, ServerInfo>
    {
        private readonly IAgentClient agentClient;

        public ServerInfoRequestHandler(IAgentClient agentClient)
        {
            this.agentClient = agentClient;
        }

        public Task<ServerInfo> Handle(ServerInfoRequest request, CancellationToken cancellationToken)
            => agentClient.SendRequest(request, cancellationToken);
    }
}