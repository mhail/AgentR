using System.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Concurrent;

namespace AgentR.Server
{
    public class InMemoryCallbackCordinator : IRequestCallbackCordinator
    {
        public static readonly IRequestCallbackCordinator Instance = new InMemoryCallbackCordinator();

        private readonly ConcurrentDictionary<long, RequestCallback> callbacks = new ConcurrentDictionary<long, RequestCallback>();

        private readonly ConcurrentDictionary<long, RequestCallback> accepted = new ConcurrentDictionary<long, RequestCallback>();

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

        public async Task<int> CreateCallback<TResult>(TaskCompletionSource<TResult> taskCompletionSource)
        {
            var id = Interlocked.Increment(ref callbacksId);

            var callback = new RequestCallback(CompletionCallback);

            while (!callbacks.TryAdd(id, callback))
            {
                await Task.Delay(1);
            }

            return id;

            bool CompletionCallback(object r, Exception ex)
            {
                if (null != ex)
                {
                    accepted.TryRemove(id, out _);
                    return taskCompletionSource.TrySetException(ex);
                }
                else
                {
                    switch (r)
                    {
                        case TResult v:
                            accepted.TryRemove(id, out _);
                            return taskCompletionSource.TrySetResult(v);
                        case Newtonsoft.Json.Linq.JObject jobject:
                            accepted.TryRemove(id, out _);
                            return taskCompletionSource.TrySetResult(jobject.ToObject<TResult>());
                        default:
                            accepted.TryRemove(id, out _);
                            return taskCompletionSource.TrySetException(new ArgumentOutOfRangeException("result", r, ""));

                    }
                }
            }
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
            public RequestCallback(Func<Object, Exception, bool> callback)
            {
                this.Callback = callback;
            }
            public Func<Object, Exception, bool> Callback { get; }

            public string AcceptedClient { get; set; }

        }
    }
}
