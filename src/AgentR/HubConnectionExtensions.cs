using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using MediatR;
using System.Threading;

namespace AgentR
{
    public static class HubConnectionExtensions
    { 
        public static void HandleRequest<T, R>(this HubConnection connection, IMediator mediator) where T : IRequest<R>
        {
            // Await the server to request registering any handlers
            connection.On(Constants.ClientRegisterHandlersMethod, async () =>{

                // register the request and response type with the server
                var registration = await connection.InvokeAsync<AgentMethodRegistration>(Constants.HubRegisterHandlerMethod, new AgentHandlerRegistration{
                    RequestType = typeof(T),
                    ResponseType = typeof(R),
                });
                
                // Remove any previous handlers for.
                connection.Remove(registration.RequestMethod);

                // Await any request from the server on the method it specified for the request and response type
                connection.On<long, T>(registration.RequestMethod, async (id, r) => {
                    Console.WriteLine($"Received {registration.RequestMethod} request<{typeof(T)}, {typeof(R)}> {id}");

                    // Accept the request, blocking any other agents
                    var accepted = await connection.InvokeAsync<bool>(Constants.HubAcceptRequestMethod, id);
                    
                    if (!accepted) {
                        // Another agent must have handled the request or at was canceled
                        Console.WriteLine($"Hub rejected request<{typeof(T)}, {typeof(R)}> {id}");
                        return;
                    } 

                    bool success;

                    try
                    {
                        // Invoke the request with MediatR
                        var result = await mediator.Send<R>(r);

                        // Send the result to the server
                        success = await connection.InvokeAsync<bool>(Constants.HubReturnResponseMethod, id, result);
                    } catch (Exception ex)
                    {
                        success = await connection.InvokeAsync<bool>(Constants.HubReturnErrorMethod, id, ex);
                    }
                    Console.WriteLine($"sent response {id} - Success: {success}");
                });

                Console.WriteLine($"Handeling <{typeof(T)}, {typeof(R)}> on {registration.RequestMethod}");

            });
        }
    }
}
