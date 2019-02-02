using System;
using System.Threading.Tasks;
using AgentR.Client;
using AgentR.Server;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Client;
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

        protected static readonly string HubPath = "/agentr";

        protected IWebHost CreateWebHost(string url)
        {
            return WebHost.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    ConfigureServices(services, url);

                    services.AddLogging(configure => configure.AddConsole());
                    services.AddSignalR();
                    services.AddMediatR(typeof(ClientServerFixture).Assembly);

                    services.AddAgentR(config =>
                    {
                        config
                            .Connection
                            .WithUrl(url + HubPath, options =>
                            {
                                ConfigureClientOptions(options);
                            });
                        var r = new Random();
                        config.ReconnectIn(_ => TimeSpan.FromSeconds(r.Next(1, 5)));

                    });

                })
                .Configure(app =>
                {
                    ConfigureApp(app, url);

                    app.UseDeveloperExceptionPage();

                    app.UseSignalR(routes =>
                    {
                        // set up the AgentR Hub
                        routes.MapHub<AgentHub>(HubPath);
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



        protected virtual void ConfigureServices(IServiceCollection services, string url)
        {

        }

        protected virtual void ConfigureApp(IApplicationBuilder app, string url)
        {

        }

        protected virtual void ConfigureClientOptions(HttpConnectionOptions options)
        {

        }
    }
}