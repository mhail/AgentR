using System.Threading.Tasks;
using MediatR;
using Xunit;

namespace IntegrationTests
{
    public abstract class ClientTests<T, R> : BaseTest<T>
        where T : ClientServerFixture
        where R : DummyRequest<R>, new()
    {
        public ClientTests(T fixture) : base(fixture) { }

        [Fact(DisplayName = "Senting request from client to server.")]
        public async Task TestSendRequest()
        {
            // Arrange
            Fixture.Client.HandleRequest<R, Unit>();
            await EnsureServerUp();
            await EnsureClientConnected();

            // Act
            var result = await Fixture.Server.SendRequest(new R());

            // Assert
            Assert.Equal(Unit.Value, result);
            DummyRequest<R>.AssertHandled();
        }
    }

    // Variants
    public class ClientTests : ClientTests<ClientServerFixture, ClientTests.Request> 
    {
        public class Request : DummyRequest<Request> { }

        public ClientTests(ClientServerFixture fixture) : base(fixture) { }
    }


    public class ClientTestsWithAuth : ClientTests<SecureClientServerFixture, ClientTestsWithAuth.Request>
    {
        public class Request : DummyRequest<Request> { }

        public ClientTestsWithAuth(SecureClientServerFixture fixture) : base(fixture) { }
    }

}