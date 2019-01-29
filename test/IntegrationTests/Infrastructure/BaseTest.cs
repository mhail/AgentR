using System;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
{
    public abstract class BaseTest : IClassFixture<ClientServerFixture>
    {
        private readonly ClientServerFixture fixture;

        public BaseTest(ClientServerFixture fixture)
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
    }
}