using System.Threading.Tasks;
using MediatR;
using Xunit;

namespace IntegrationTests
{
    public class ClientTestsRequest : DummyRequest<ClientTestsRequest> { }

    public abstract class ClientTests<T> : BaseTest<T> where T : ClientServerFixture
    {
        public ClientTests(T fixture) : base(fixture) { }

        [Fact(DisplayName = "Senting request from client to server.")]
        public async Task TestSendRequest()
        {
            // Arrange
            Fixture.Client.HandleRequest<ClientTestsRequest, Unit>();
            await EnsureServerUp();
            await EnsureClientConnected();

            // Act
            var result = await Fixture.Server.SendRequest(new ClientTestsRequest());

            // Assert
            Assert.Equal(Unit.Value, result);
            ClientTestsRequest.AssertHandled();
        }
    }

    public class ClientTests : ClientTests<ClientServerFixture> 
    {
         public ClientTests(ClientServerFixture fixture) : base(fixture) { }
    }


    public class ClientTestsWithAuth : ClientTests<SecureClientServerFixture>
    {
        public ClientTestsWithAuth(SecureClientServerFixture fixture) : base(fixture) { }
    }

}