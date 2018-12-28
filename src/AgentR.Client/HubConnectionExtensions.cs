using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using MediatR;
using System.Threading;

namespace AgentR.Client
{
    public static class HubConnectionExtensions
    {
        public static void HandleRequest<TRequest, TResponse>(this HubConnection connection, IMediator mediator) where TRequest : IRequest<TResponse>
        {
            // Await the server to request registering any handlers
            connection.OnRegisterHandlers(async () =>
            {

                await connection.RegisterHandler<TRequest, TResponse>(async (callbackId, request) =>
                {
                    // Accept the request, blocking any other agents
                    var accepted = await connection.AcceptRequest(callbackId);

                    if (!accepted)
                    {
                        // Another agent must have handled the request or at was canceled
                        Diagnostics.Tracer.TraceInformation($"Hub canceled request<{typeof(TRequest)}, {typeof(TResponse)}> {callbackId}");
                        return;
                    }

                    bool success = await connection.SendResponse(callbackId, () => mediator.Send<TResponse>(request));

                    Diagnostics.Tracer.TraceInformation($"Sent response {callbackId} - Success: {success}");

                });
            });
        }

        public static Task<TResponse> SendRequest<TResponse>(this HubConnection connection, IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))  
        {
            return connection.InvokeAsync<TResponse>(Constants.HubAgentRequestMethod, request.GetType(), typeof(TResponse), request, cancellationToken); 
        }

        internal static async Task<bool> AcceptRequest(this HubConnection connection, int callbackId)
        {
            var result = await connection.InvokeAsync<bool>(Constants.HubAcceptRequestMethod, callbackId);
            return result;
        }

        internal static async Task<bool> ReturnResponse(this HubConnection connection, int callbackId, object response)
        {
            var result = await connection.InvokeAsync<bool>(Constants.HubReturnResponseMethod, callbackId, response);
            return result;
        }

        internal static async Task<bool> ReturnError(this HubConnection connection, int callbackId, Exception ex)
        {
            var result = await connection.InvokeAsync<bool>(Constants.HubReturnErrorMethod, callbackId, ex);
            return result;
        }

        internal static async Task<IDisposable> RegisterHandler<TRequest, TResponse>(this HubConnection connection, Action<int, TRequest> callback) where TRequest : IRequest<TResponse>
        {
            // register the request and response type with the server
            var registration = await connection.InvokeAsync<AgentMethodRegistration>(Constants.HubRegisterHandlerMethod, new AgentHandlerRegistration
            {
                RequestType = typeof(TRequest),
                ResponseType = typeof(TResponse),
            });

            // Remove any previous handlers for.
            connection.Remove(registration.RequestMethod);

            // Await any request from the server on the method it specified for the request and response type
            var result = connection.On<int, TRequest>(registration.RequestMethod, (callbackId, request) =>
            {
                Diagnostics.Tracer.TraceInformation($"Received request<{typeof(TRequest)}, {typeof(TResponse)}> {callbackId}");
                callback(callbackId, request);
            });

            Diagnostics.Tracer.TraceInformation($"Handeling <{typeof(TRequest)}, {typeof(TResponse)}> on {registration.RequestMethod}");

            return result;
        }

        internal static IDisposable OnRegisterHandlers(this HubConnection connection, Action callback)
        {
            return connection.On(Constants.ClientRegisterHandlersMethod, callback);
        }

        internal static async Task<bool> SendResponse<TResponse>(this HubConnection connection, int callbackId, Func<Task<TResponse>> callback)
        {
            try
            {
                var response = callback();

                // Send the result to the server
                return await connection.ReturnResponse(callbackId, response);
            }
            catch (Exception ex)
            {
                return await connection.ReturnError(callbackId, ex);
            }
        }
    }
}
