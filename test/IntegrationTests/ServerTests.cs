using System.Threading.Tasks;
using MediatR;
using Xunit;

namespace IntegrationTests
{
    public abstract class ServerTests<T, R> : BaseTest<T>
        where T : ClientServerFixture
        where R : DummyRequest<R>, new()
    {
        public ServerTests(T fixture) : base(fixture) { }

        [Fact(DisplayName = "Senting request from server to client.")]
        public async Task TestSendRequest()
        {
            // Arrange
            await EnsureServerUp();
            await EnsureClientConnected();

            // Act
            var result = await Fixture.Client.SendRequest(new R());

            // Assert
            Assert.Equal(Unit.Value, result);

            DummyRequest<R>.AssertHandled();
        }
    }

    public class ServerTests : ServerTests<ClientServerFixture, ServerTests.Request>
    {
        public class Request : DummyRequest<Request> { }

        public ServerTests(ClientServerFixture fixture) : base(fixture) { }
    }

    public class ServerTestsWithAuth : ServerTests<SecureClientServerFixture, ServerTestsWithAuth.Request>
    {
        public class Request : DummyRequest<Request> { }

        public ServerTestsWithAuth(SecureClientServerFixture fixture) : base(fixture) { }
    }
}