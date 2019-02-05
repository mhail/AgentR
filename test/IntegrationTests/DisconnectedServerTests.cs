using System;
using System.Threading.Tasks;
using MediatR;
using Xunit;
using AgentR.Client;

namespace IntegrationTests
{
    public abstract class DisconnectedServerTests<T, R> : BaseTest<T> 
        where T : ClientServerFixture 
        where R : DummyRequest<R>, new()
    {
        public DisconnectedServerTests(T fixture) : base(fixture) { }

        [Fact(DisplayName = "Test server is down and client is waiting.")]
        public async Task TestSendRequest()
        {
            // Arrange
            Assert.False(Fixture.Client.IsConnected);
            await EnsureServerDown();
            Fixture.Client.HandleRequest<R, Unit>();

            // Queue up the client to try and connect to the server
            var clientConnectTask = Fixture.Client.TryConnect();

            // Act
            await Task.Delay(TimeSpan.FromSeconds(1));
            await Fixture.StartServer();
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Send a request to the client
            var sendRequestTask = Fixture.Server.SendRequest(new R());

            // Wait for the client to connect and the result to process
            await clientConnectTask;
            var result = await sendRequestTask;

            // Assert
            Assert.True(Fixture.Client.IsConnected);
            Assert.Equal(Unit.Value, result);
            DummyRequest<R>.AssertHandled();

        }
    }

    public class DisconnectedServerTests : DisconnectedServerTests<ClientServerFixture, DisconnectedServerTests.Request>
    {
        public class Request : DummyRequest<Request> { }

        public DisconnectedServerTests(ClientServerFixture fixture) : base(fixture) { }
    }

    public class DisconnectedServerTestsWithAuth : DisconnectedServerTests<SecureClientServerFixture, DisconnectedServerTestsWithAuth.Request>
    {
        public class Request : DummyRequest<Request> { }

        public DisconnectedServerTestsWithAuth(SecureClientServerFixture fixture) : base(fixture) { }
    }
}