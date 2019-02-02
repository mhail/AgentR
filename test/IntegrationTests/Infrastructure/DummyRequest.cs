using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Xunit;

namespace IntegrationTests
{
    /// <summary>
    /// Dummy request for tracking when Handle is called.
    /// </summary>
    public abstract class DummyRequest<T> : IRequest, IRequestHandler<T> where T : IRequest
    {
        private static ConcurrentDictionary<Type, bool> values = new ConcurrentDictionary<Type, bool>();

        public static bool HandleCalled=> values.TryGetValue(typeof(T), out bool v) ? v : false;

        public DummyRequest()
        {
            values.AddOrUpdate(typeof(T), false, (t, v) => false);
        }

        public Task<Unit> Handle(T request, CancellationToken cancellationToken)
        {
            values.AddOrUpdate(typeof(T), true, (t, v) => true);

            return Unit.Task;
        }

        public static void AssertHandled() => Assert.True(HandleCalled, $"{nameof(T)} not handled");
    }

    public class DummyRequestTests
    {
        class TestRequest1 : DummyRequest<TestRequest1> { }
        class TestRequest2 : DummyRequest<TestRequest2> { }

        [Fact]
        public async Task TestHandledCalled()
        {
            var request1 = new TestRequest1();
            var request2 = new TestRequest2();

            Assert.False(TestRequest1.HandleCalled);
            Assert.False(TestRequest2.HandleCalled);

            await request1.Handle(request1, CancellationToken.None);

            Assert.True(TestRequest1.HandleCalled);
            Assert.False(TestRequest2.HandleCalled);

            await request2.Handle(request2, CancellationToken.None);

            Assert.True(TestRequest1.HandleCalled);
            Assert.True(TestRequest2.HandleCalled);

            TestRequest1.AssertHandled();
            TestRequest2.AssertHandled();
        }
    }
}