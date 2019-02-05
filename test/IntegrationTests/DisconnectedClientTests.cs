using System;
using System.Threading.Tasks;
using MediatR;
using Xunit;

namespace IntegrationTests
{
    public abstract class DisconnectedClientTests<T, R> : BaseTest<T>
        where T : ClientServerFixture
        where R : DummyRequest<R>, new()
    {
        public DisconnectedClientTests(T fixture) : base(fixture) { }

        [Fact(DisplayName = "Test server has inqueued request and client connects.")]
        public async Task TestSendRequest()
        {
            // Arrange
            Fixture.Client.HandleRequest<R, Unit>();
            await EnsureServerUp();
            var sendRequestTask = Fixture.Server.SendRequest(new R());

            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.False(Fixture.Client.IsConnected);

            // Act
            await EnsureClientConnected();

            var result = await sendRequestTask;

            // Assert
            Assert.Equal(Unit.Value, result);
            DummyRequest<R>.AssertHandled();
        }
    }

    // Variants

    public class DisconnectedClientTests : DisconnectedClientTests<ClientServerFixture, DisconnectedClientTests.Request> 
    {
        public class Request : DummyRequest<Request> { }

        public DisconnectedClientTests(ClientServerFixture fixture) : base(fixture) { }
    }

    public class DisconnectedClientTestsWithAuth : DisconnectedClientTests<SecureClientServerFixture, DisconnectedClientTestsWithAuth.Request>
    {
        public class Request : DummyRequest<Request> { }

        public DisconnectedClientTestsWithAuth(SecureClientServerFixture fixture) : base(fixture) { }
    }
}