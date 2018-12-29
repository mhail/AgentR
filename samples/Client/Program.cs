using System;
using MediatR;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using AgentR.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Server.Requests;

namespace Client
{
    class Program : MediatorApp<Program>
    {
        static Task Main(string[] args) => SendMainRequest(args);

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            base.ConfigureServices(serviceCollection);

            serviceCollection.AddAgentR(config => {
                config
                    .Connection
                    .WithUrl("http://localhost:5000/agentr");
                    /*.ConfigureLogging(logging =>
                    {
                        logging.SetMinimumLevel(LogLevel.Information);
                        logging.AddConsole();
                    });*/
                config.HandleRequest<SampleRequest, Unit>();
                config.HandleRequest<SampleRequest2, Unit>();
                var r = new Random();
                config.ReconnectIn(() => TimeSpan.FromSeconds(r.Next(1, 5)));
            });
        }
    }
}
