using System.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Concurrent;

namespace AgentR.Server
{
    public class InMemoryCallbackCordinator : IRequestCallbackCordinator
    {
        public static readonly IRequestCallbackCordinator Instance = new InMemoryCallbackCordinator();

        private readonly ConcurrentDictionary<int, RequestCallback> callbacks = new ConcurrentDictionary<int, RequestCallback>();

        private readonly ConcurrentDictionary<int, RequestCallback> accepted = new ConcurrentDictionary<int, RequestCallback>();

        private int callbacksId = 1;

        public async Task<bool> Accept(int id, string connectionId)
        {
            if (callbacks.TryRemove(id, out RequestCallback callback))
            {
                while (!accepted.TryAdd(id, callback))
                {
                    await Task.Delay(1);
                }
                callback.AcceptedClient = connectionId;

                Diagnostics.Tracer.TraceInformation($"Request {id} accepted by {callback.AcceptedClient}");

                return true;
            }
            return false;
        }

        public async Task<int> CreateCallback<TRequest, TResponse>(TRequest request, TaskCompletionSource<TResponse> taskCompletionSource)
        {
            var id = Interlocked.Increment(ref callbacksId);

            bool CompletionCallback(object r, Exception ex)
            {
                try
                {
                    if (null != ex) return taskCompletionSource.TrySetException(ex);

                    switch (r)
                    {
                        case TResponse v:
                            accepted.TryRemove(id, out _);
                            return taskCompletionSource.TrySetResult(v);
                        case Newtonsoft.Json.Linq.JObject jobject:
                            accepted.TryRemove(id, out _);
                            return taskCompletionSource.TrySetResult(jobject.ToObject<TResponse>());
                        default:
                            accepted.TryRemove(id, out _);
                            return taskCompletionSource.TrySetException(new ArgumentOutOfRangeException("result", r, ""));

                    }

                }
                finally
                {
                    accepted.TryRemove(id, out _);
                }
            }

            var callback = new RequestCallback<TRequest>(id, CompletionCallback, request);

            while (!callbacks.TryAdd(id, callback))
            {
                await Task.Delay(1);
            }

            return id;
        }

        public Task<bool> Error(int id, Exception ex, string connectionId)
        {
            if (accepted.TryGetValue(id, out RequestCallback callback))
            {
                if (connectionId != callback.AcceptedClient)
                {
                    throw new InvalidOperationException("Response was not accepted by client");
                }
                return Task.FromResult(callback.Callback(null, ex));
            }
            return Task.FromResult(false);
        }

        public Task<bool> IsAccepted(int id)
        {
            var isAccepted = accepted.ContainsKey(id) || !callbacks.ContainsKey(id);
            return Task.FromResult(isAccepted);
        }

        public Task<bool> Response(int id, object response, string connectionId)
        {
            if (accepted.TryGetValue(id, out RequestCallback callback))
            {
                if (connectionId != callback.AcceptedClient)
                {
                    throw new InvalidOperationException("Response was not accepted by client");
                }
                return Task.FromResult(callback.Callback(response, null));
            }
            return Task.FromResult(false);
        }

        internal class RequestCallback
        {
            private readonly Func<Object, Exception, bool> callback;
            private readonly int id;

            public RequestCallback(int id, Func<Object, Exception, bool> callback)
            {
                this.id = id;
                this.callback = callback;
            }

            public int Id => this.id;

            public Func<Object, Exception, bool> Callback => this.callback;

            public string AcceptedClient { get; set; }

        }

        internal class RequestCallback<TRequest> : RequestCallback
        {
            private readonly TRequest request;
            public RequestCallback(int id, Func<Object, Exception, bool> callback, TRequest request)
                : base(id, callback)
            {
                this.request = request;
            }

            public TRequest Request => this.request;
        }
    }
}
