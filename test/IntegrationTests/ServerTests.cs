using System.Threading.Tasks;
using MediatR;
using Xunit;

namespace IntegrationTests
{
    public class ServerTests : BaseTest
    {
        class ServerRequest1 : DummyRequest<ServerRequest1> { }

        public ServerTests(ClientServerFixture fixture) : base(fixture) { }

        [Fact(DisplayName = "Senting request from server to client.")]
        public async Task TestSendRequest()
        {
            // Arrange
            await EnsureServerUp();
            await EnsureClientConnected();

            // Act
            var result = await Fixture.Client.SendRequest(new ServerRequest1());

            // Assert
            Assert.Equal(Unit.Value, result);

            ServerRequest1.AssertHandled();
        }
    }
}