using System.Threading.Tasks;
using MediatR;
using Xunit;

namespace IntegrationTests
{
    public class ClientTests : BaseTest
    {
        public class ClientRequest1 : DummyRequest<ClientRequest1> { }

        public ClientTests(ClientServerFixture fixture) : base(fixture) { }

        [Fact(DisplayName = "Senting request from client to server.")]
        public async Task TestSendRequest()
        {
            // Arrange
            Fixture.Client.HandleRequest<ClientRequest1, Unit>();
            await EnsureServerUp();
            await EnsureClientConnected();

            // Act
            var result = await Fixture.Server.SendRequest(new ClientRequest1());

            // Assert
            Assert.Equal(Unit.Value, result);
            ClientRequest1.AssertHandled();
        }
    }
}