using System;
using MediatR;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Client
{
    class Program : MediatorApp<Program>
    {
        static Task Main(string[] args) => SendMainRequest(args);

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            base.ConfigureServices(serviceCollection);
        }
    }
}
