using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Server.Requests;

namespace Client.Handlers
{
    public class SampleRequestHandler : IRequestHandler<SampleRequest> {
      public async Task<Unit> Handle(SampleRequest request, CancellationToken cancellationToken)
      {
        Console.WriteLine($"SampleRequest Property1: {request.Property1}");

        return Unit.Value;
      }
  }

  public class SampleRequestHandler2 : IRequestHandler<SampleRequest2> {
      public async Task<Unit> Handle(SampleRequest2 request, CancellationToken cancellationToken)
      {
        Console.WriteLine($"SampleRequest2 Property2: {request.Property2}");

        return Unit.Value;
      }
    }
}