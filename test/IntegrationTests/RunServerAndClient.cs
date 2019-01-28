using NUnit.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using System;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Builder;
using AgentR.Server;
using AgentR.Client;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace IntegrationTests
{
    public class RunServerAndClient
    {
        static readonly int Port = 5999;

        static readonly string ServerUrl = $"http://{System.Net.IPAddress.Loopback}:{Port}";


        IWebHost host;

        [SetUp]
        public void Setup()
        {
            host = CreateWebHost(ServerUrl);
        }

        static IWebHost CreateWebHost(string url)
        {
            string hubPath = "/agentr";

            return WebHost.CreateDefaultBuilder()
                .ConfigureServices((services) => {
                    services.AddLogging(configure => configure.AddConsole());
                    services.AddSignalR();
                    services.AddMediatR(typeof(RunServerAndClient).Assembly);

                    services.AddAgentR(config => {
                        config
                            .Connection
                            .WithUrl(url + hubPath);
                        var r = new Random();
                        config.ReconnectIn(_ => TimeSpan.FromSeconds(r.Next(1, 5)));
                    });

                })
                .Configure(app => {
                    app.UseDeveloperExceptionPage();

                    app.UseSignalR(routes =>
                    {
                        // set up the AgentR Hub
                        routes.MapHub<AgentHub>(hubPath);
                    });
                })
                .Start(url);
        }

        [TearDown]
        public void TearDown()
        {
            host.StopAsync(TimeSpan.FromSeconds(1)).Wait();
        }

        class ServerRequest : IRequest, IRequestHandler<ServerRequest>
        {
            public static bool HandleCalled { get; set; }

            public Task<Unit> Handle(ServerRequest request, CancellationToken cancellationToken)
            {
                HandleCalled = true;

                return Unit.Task;
            }
        }

        [Test]
        public async Task TestSendRequestToServer()
        {
            // Arrange
            var client = host.Services.GetService<IAgentClient>();

            await client.StartAsync();

            Assert.IsTrue(client.IsConnected, "Client Not Connected");

            Assert.IsFalse(ServerRequest.HandleCalled);

            // Act
            var result = await client.SendRequest(new ServerRequest());

            // Assert
            Assert.AreEqual(Unit.Value, result);
            Assert.IsTrue(ServerRequest.HandleCalled, "Server Method Not Called");

        }

        class ClientRequest : IRequest, IRequestHandler<ClientRequest>
        {
            public static bool HandleCalled { get; set; }

            public Task<Unit> Handle(ClientRequest request, CancellationToken cancellationToken)
            {
                HandleCalled = true;

                return Unit.Task;
            }
        }

        [Test]
        public async Task TestSendRequestToClient()
        {
            // Arrange
            var client = host.Services.GetService<IAgentClient>();

            client.HandleRequest<ClientRequest, Unit>();

            await client.StartAsync();

            Assert.IsTrue(client.IsConnected, "Client Not Connected");

            var hub = host.Services.GetService<IHubContext<AgentHub>>();

            Assert.IsNotNull(hub);

            var handler = new AgentHandler<ClientRequest, Unit>(hub);

            Assert.IsFalse(ClientRequest.HandleCalled);

            // Act
            var result = await handler.Handle(new ClientRequest());

            // Assert
            Assert.AreEqual(Unit.Value, result);
            Assert.IsTrue(ClientRequest.HandleCalled, "Client Method Not Called");
        }
    }
}