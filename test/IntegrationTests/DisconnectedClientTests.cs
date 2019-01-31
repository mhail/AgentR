using System;
using System.Threading.Tasks;
using MediatR;
using Xunit;

namespace IntegrationTests
{
    public class DisconnectedClientTests : BaseTest
    {
        public class ClientRequest1 : DummyRequest<ClientRequest1> { }

        public DisconnectedClientTests(ClientServerFixture fixture) : base(fixture) { }

        [Fact(DisplayName = "Test server has inqueued request and client connects.")]
        public async Task TestSendRequest()
        {
            // Arrange
            Fixture.Client.HandleRequest<ClientRequest1, Unit>();
            await EnsureServerUp();
            var sendRequestTask = Fixture.Server.SendRequest(new ClientRequest1());

            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.False(Fixture.Client.IsConnected);

            // Act
            await EnsureClientConnected();

            var result = await sendRequestTask;

            // Assert
            Assert.Equal(Unit.Value, result);
            ClientRequest1.AssertHandled();
        }
    }
}