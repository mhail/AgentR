using System;
using System.Threading.Tasks;
using MediatR;
using Xunit;
using AgentR.Client;

namespace IntegrationTests
{
    public class DisconnectedServerTests : BaseTest
    {
        public class ServerRequest1 : DummyRequest<ServerRequest1> { }

        public DisconnectedServerTests(ClientServerFixture fixture) : base(fixture) { }

        [Fact(DisplayName = "Test server is down and client is waiting.")]
        public async Task TestSendRequest()
        {
            // Arrange
            Assert.False(Fixture.Client.IsConnected);
            await EnsureServerDown();
            Fixture.Client.HandleRequest<ServerRequest1, Unit>();

            // Queue up the client to try and connect to the server
            var clientConnectTask = Fixture.Client.TryConnect();

            // Act
            await Task.Delay(TimeSpan.FromSeconds(1));
            await Fixture.StartServer();
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Send a request to the client
            var sendRequestTask = Fixture.Server.SendRequest(new ServerRequest1());

            // Wait for the client to connect and the result to process
            await clientConnectTask;
            var result = await sendRequestTask;

            // Assert
            Assert.True(Fixture.Client.IsConnected);
            Assert.Equal(Unit.Value, result);
            ServerRequest1.AssertHandled();

        }
    }
}