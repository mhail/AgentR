using System.Threading.Tasks;
using MediatR;
using Xunit;

namespace IntegrationTests
{
    public class ServerTestRequest : DummyRequest<ServerTestRequest> { }

    public abstract class ServerTests<T> : BaseTest<T> where T : ClientServerFixture
    {
        public ServerTests(T fixture) : base(fixture) { }

        [Fact(DisplayName = "Senting request from server to client.")]
        public async Task TestSendRequest()
        {
            // Arrange
            await EnsureServerUp();
            await EnsureClientConnected();

            // Act
            var result = await Fixture.Client.SendRequest(new ServerTestRequest());

            // Assert
            Assert.Equal(Unit.Value, result);

            ServerTestRequest.AssertHandled();
        }
    }

    public class ServerTests : ServerTests<ClientServerFixture>
    {
        public ServerTests(ClientServerFixture fixture) : base(fixture) { }
    }

    public class ServerTestsWithAuth : ServerTests<SecureClientServerFixture>
    {
        public ServerTestsWithAuth(SecureClientServerFixture fixture) : base(fixture) { }
    }
}