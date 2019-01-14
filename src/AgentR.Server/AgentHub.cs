using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MediatR;
using System;
using System.Threading;
using System.Collections.Concurrent;

namespace AgentR.Server
{
    public interface IRequestCallbackCordinator
    {
        Task<int> CreateCallback<TResult>(TaskCompletionSource<TResult> taskCompletionSource);
        Task<bool> Accept(int id, string ConnectionId);
        Task<bool> Response(int id, object response, string connectionId);
        Task<bool> Error(int id, Exception ex, string connectionId);
    }

    public class AgentHub : Hub
    {
        private readonly IRequestCallbackCordinator cordinator;
        private readonly IMediator mediator;

        internal AgentHub(IMediator mediator, IRequestCallbackCordinator cordinator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(IMediator));
            this.cordinator = cordinator ?? throw new ArgumentNullException(nameof(IRequestCallbackCordinator));
        }

        public AgentHub(IMediator mediator) : this(mediator, InMemoryCallbackCordinator.Instance)
        {
        }


        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync(Constants.ClientRegisterHandlersMethod);

            await base.OnConnectedAsync();
        }

        [HubMethodName(Constants.HubRegisterHandlerMethod)]
        public async Task<AgentMethodRegistration> RegisterHandler(AgentHandlerRegistration registration)
        {
            var group = GetGroupName(registration.RequestType, registration.ResponseType);

            Diagnostics.Tracer.TraceInformation($"{Context.ConnectionId} handeling {group}");

            await Groups.AddToGroupAsync(Context.ConnectionId, group);

            return new AgentMethodRegistration{
                RequestMethod = GetAgentMethodName(registration.RequestType, registration.ResponseType),
            };
        }

        /// <summary>
        /// Called by the client to accept handeling a request
        /// </summary>
        /// <returns>True to instruct the client to fufill the request. 
        /// Only one client will recieve a successfull result
        /// </returns>
        /// <param name="callbackId">Callback identifier.</param>
        [HubMethodName(Constants.HubAcceptRequestMethod)]
        public Task<bool> AcceptRequest(int callbackId) {

            return cordinator.Accept(callbackId, Context.ConnectionId);
        }

        [HubMethodName(Constants.HubReturnResponseMethod)]
        public Task<bool> ResultResponse(int id, object response)
        {
            Diagnostics.Tracer.TraceInformation("Received response");

            return cordinator.Response(id, response, Context.ConnectionId);
        }

        [HubMethodName(Constants.HubReturnErrorMethod)]
        public Task<bool> ResultError(int id, Exception ex)
        {
            Diagnostics.Tracer.TraceInformation("Received error");

            return cordinator.Error(id, ex, Context.ConnectionId);
        }

        [HubMethodName(Constants.HubAgentRequestMethod)]
        public async Task<object> ClientRequest(Type requestType, Type responseType, object request)
        {
            request = CastToType(requestType, request);

            Diagnostics.Tracer.TraceInformation($"Received clientRequest<{requestType}, {responseType}> {request}");

            var result = await mediator.Send(responseType, request);

            return result;
        }

        [HubMethodName(Constants.HubAgentNotificationMethod)]
        public async Task ClientNotification(Type notificationType, object notification)
        {
            notification = CastToType(notificationType, notification);

            await mediator.Publish(notification);
        }

        internal static object CastToType(Type type, object obj)
        {
            switch (obj)
            {
                case Newtonsoft.Json.Linq.JObject jobject:

                    obj = jobject.ToObject(type);
                    break;
            }

            return obj;
        }

        public static string GetGroupName(Type request, Type result)
        {
            return $"handles:{request.FullName}:{result.FullName}";
        }

        public static string GetAgentMethodName(Type request, Type result)
        {
            return $"request_{request.FullName}_{result.FullName}";
        }
    }
}
