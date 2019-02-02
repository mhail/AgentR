using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
{
   

    public abstract class BaseTest<T> : IClassFixture<T> where T : ClientServerFixture
    {
        private readonly T fixture;

        public BaseTest(T fixture)
        {
            this.fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        protected ClientServerFixture Fixture => this.fixture;

        protected void AssertClientConnected() => Assert.True(Fixture.Client.IsConnected, "Client Not Connected");

        protected async Task EnsureClientConnected()
        {
            await Fixture.Client.StartAsync();

            AssertClientConnected();
        }

        protected async Task EnsureServerUp()
        {
            await Fixture.StartServer();

            var client = HttpClientFactory.Create();

            var request = await client.GetStringAsync(Fixture.ServerUrl);

            Assert.Equal("Ok", request);
        }

        protected async Task EnsureServerDown()
        {
            await Fixture.StopServer();

            var client = HttpClientFactory.Create();

            await Assert.ThrowsAsync<HttpRequestException>(()=> client.GetStringAsync(Fixture.ServerUrl));
        }
    }

    public class BaseTest : BaseTest<ClientServerFixture>
    {
        public BaseTest(ClientServerFixture fixture) : base(fixture)
        {
        }
    }
}