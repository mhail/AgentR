using System;
using System.Threading.Tasks;
using MediatR;
using Xunit;

namespace IntegrationTests
{
    public class DisconnectedClientRequest : DummyRequest<DisconnectedClientRequest> { }

    public abstract class DisconnectedClientTests<T> : BaseTest<T> where T : ClientServerFixture
    {
        public DisconnectedClientTests(T fixture) : base(fixture) { }

        [Fact(DisplayName = "Test server has inqueued request and client connects.")]
        public async Task TestSendRequest()
        {
            // Arrange
            Fixture.Client.HandleRequest<DisconnectedClientRequest, Unit>();
            await EnsureServerUp();
            var sendRequestTask = Fixture.Server.SendRequest(new DisconnectedClientRequest());

            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.False(Fixture.Client.IsConnected);

            // Act
            await EnsureClientConnected();

            var result = await sendRequestTask;

            // Assert
            Assert.Equal(Unit.Value, result);
            DisconnectedClientRequest.AssertHandled();
        }
    }

    public class DisconnectedClientTests : DisconnectedClientTests<ClientServerFixture> 
    {
        public DisconnectedClientTests(ClientServerFixture fixture) : base(fixture) { }
    }

    public class DisconnectedClientTestsWithAuth : DisconnectedClientTests<SecureClientServerFixture>
    {
        public DisconnectedClientTestsWithAuth(SecureClientServerFixture fixture) : base(fixture) { }
    }
}