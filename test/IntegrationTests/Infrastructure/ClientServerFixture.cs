using System;
using System.Threading.Tasks;
using AgentR.Client;
using AgentR.Server;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IntegrationTests
{
    public class ClientServerFixture : IDisposable
    {
        static int Port = 5800;

        private readonly IWebHost host;

        private readonly Lazy<IAgentClient> client;
        public IAgentClient Client => this.client.Value;

        private readonly Lazy<IHubContext<AgentHub>> server;
        public IHubContext<AgentHub> Server => this.server.Value;

        private readonly string serverUrl;

        public string ServerUrl => this.serverUrl;

        public ClientServerFixture()
        {
            this.serverUrl = $"http://{System.Net.IPAddress.Loopback}:{Port++}";

            host = CreateWebHost(serverUrl);

            client = new Lazy<IAgentClient>(() => host.Services.GetService<IAgentClient>());
            server = new Lazy<IHubContext<AgentHub>>(() => host.Services.GetService<IHubContext<AgentHub>>());
        }

        public void Dispose()
        {
            host.StopAsync(TimeSpan.FromSeconds(1)).Wait();
        }

        public Task StartServer() => host.StartAsync();
        public Task StopServer() => host.StopAsync(TimeSpan.FromSeconds(10));

        static IWebHost CreateWebHost(string url)
        {
            string hubPath = "/agentr";

            return WebHost.CreateDefaultBuilder()
                .ConfigureServices((services) =>
                {
                    services.AddLogging(configure => configure.AddConsole());
                    services.AddSignalR();
                    services.AddMediatR(typeof(ClientServerFixture).Assembly);

                    services.AddAgentR(config =>
                    {
                        config
                            .Connection
                            .WithUrl(url + hubPath);
                        var r = new Random();
                        config.ReconnectIn(_ => TimeSpan.FromSeconds(r.Next(1, 5)));
                    });

                })
                .Configure(app =>
                {
                    app.UseDeveloperExceptionPage();

                    app.UseSignalR(routes =>
                    {
                        // set up the AgentR Hub
                        routes.MapHub<AgentHub>(hubPath);
                    });

                    app.Run(async context =>
                    {
                        await context.Response.WriteAsync("Ok");
                    });
                })
                .UseUrls(url)
                .Build();
                //.Start(url);
        }
    }
}