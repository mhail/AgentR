using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR {
    
    // Note: This class is not needed for a AgentR client it just 
    // encapsulates MediatR and Dependency Injection
    abstract class MediatorApp<T> : IConfigureServiceCollection where T : IConfigureServiceCollection, new()
    {
        private static readonly Lazy<IMediator> mediator = new Lazy<IMediator>(CreateMediator);

        public static IMediator Mediator => mediator.Value;

        static IMediator CreateMediator()
        {
            var cfg = new T();

            var serviceCollection = new ServiceCollection();

            cfg.ConfigureServices(serviceCollection);

            var provider = serviceCollection.BuildServiceProvider();

            return provider.GetService<IMediator>();
        }

        protected static Task SendMainRequest(string[] args)
            => Mediator.Send<MediatR.Unit>(new MainRequest { Args = args });

        public virtual void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddMediatR(typeof(T).Assembly);
        }
    }

    interface IConfigureServiceCollection
    {
        void ConfigureServices(IServiceCollection serviceCollection);
    }
    
    public class MainRequest : IRequest
    {
        public IEnumerable<string> Args { get; set; }
    }
}