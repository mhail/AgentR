using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Server.Requests;

namespace Client.Handlers
{
    public class SampleRequestHandler : IRequestHandler<SampleRequest>
    {
        static readonly Random rand = new Random();

        public async Task<Unit> Handle(SampleRequest request, CancellationToken cancellationToken)
        {
            await Task.Delay(1 + rand.Next(99));

            Console.WriteLine($"SampleRequest Property1: {request.Property1}");

            return Unit.Value;
        }
    }

    public class SampleRequestHandler2 : IRequestHandler<SampleRequest2>
    {
        public async Task<Unit> Handle(SampleRequest2 request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"SampleRequest2 Property2: {request.Property2}");

            return Unit.Value;
        }
    }
}