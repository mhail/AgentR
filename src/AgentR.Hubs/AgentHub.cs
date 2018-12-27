using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MediatR;
using System;
using System.Threading;
using System.Collections.Concurrent;

namespace AgentR.Hubs
{
    public class AgentHub : Hub
    {
        internal class RequestCallback
        {
            public RequestCallback(Func<Object, Exception, bool> callback) {
                this.Callback = callback;
            }
            public Func<Object, Exception, bool> Callback { get;}

            public string AcceptedClient {get; set; }
        }


        static ConcurrentDictionary<long, RequestCallback> callbacks = new ConcurrentDictionary<long, RequestCallback>();
        
        static ConcurrentDictionary<long, RequestCallback> accepted = new ConcurrentDictionary<long, RequestCallback>();
        
        static long callbacksId = 1;

        public AgentHub()
        {
        }


        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("registerHandlers");

            await base.OnConnectedAsync();
        }

        [HubMethodName(Constants.HubRegisterHandlerMethod)]
        public async Task<AgentMethodRegistration> handeling(AgentHandlerRegistration registration)
        {
            var group = GetGroupName(registration.RequestType, registration.ResponseType);

            Console.WriteLine($"{Context.ConnectionId} handeling {group}");

            await Groups.AddToGroupAsync(Context.ConnectionId, group);

            return new AgentMethodRegistration{
                RequestMethod = GetAgentMethodName(registration.RequestType, registration.ResponseType),

            };
        }

        [HubMethodName(Constants.HubAcceptRequestMethod)]
        public async Task<bool> accept(long id) {
            if (callbacks.TryRemove(id, out RequestCallback callback)) {
                while (!accepted.TryAdd(id, callback)) {
                    await Task.Delay(1);
                }
                callback.AcceptedClient = Context.ConnectionId;

                Console.WriteLine($"Request {id} accepted by {callback.AcceptedClient}");

                return true;
            }
            return false;
        }

        public bool response(long id, object result)
        {
            Console.WriteLine("Received response");

            if (accepted.TryGetValue(id, out RequestCallback callback))
            {
                return callback.Callback(result, null);
            }
            return false;
        }

        public bool responseError(long id, Exception ex)
        {
            Console.WriteLine("Received error");

            if (accepted.TryGetValue(id, out RequestCallback callback))
            {
                return callback.Callback(null, ex);
            }
            return false;
        }

        public static string GetGroupName(Type request, Type result)
        {
            return $"handles:{request.FullName}:{result.FullName}";
        }

        public static string GetAgentMethodName(Type request, Type result)
        {
            return $"request_{request.FullName}_{result.FullName}";
        }

        

        public static async Task<R> Handle<T, R>(IHubContext<AgentHub> hub, T request, CancellationToken cancellationToken) where T : IRequest<R>
        {
            long id = Interlocked.Increment(ref callbacksId);

            var completion = new TaskCompletionSource<R>();
            var group = GetGroupName(typeof(T), typeof(R));

            while (! callbacks.TryAdd(id, new RequestCallback( CompletionCallback)) ) {
                await Task.Delay(1);
            }

            var client = hub.Clients.Group(group);

            Console.WriteLine($"sending request {id}");

            await client.SendAsync(GetAgentMethodName(typeof(T), typeof(R)), id, request, cancellationToken);

            var result = await completion.Task;

            return result;

            bool CompletionCallback(object r, Exception ex)
            {
                if (null != ex)
                {
                    accepted.TryRemove(id, out _);
                    return completion.TrySetException(ex);
                }
                else
                {
                    switch (r)
                    {
                        case R v:
                            accepted.TryRemove(id, out _);
                            return completion.TrySetResult(v);
                        case Newtonsoft.Json.Linq.JObject jobject:
                            accepted.TryRemove(id, out _);
                            return completion.TrySetResult(jobject.ToObject<R>());
                        default:
                            accepted.TryRemove(id, out _);
                            return completion.TrySetException(new ArgumentOutOfRangeException("result", r, ""));

                    }
                }
            }
        }

    }
}
