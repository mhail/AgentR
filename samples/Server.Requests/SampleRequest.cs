using System;
using MediatR;

namespace Server.Requests
{
    // These are the request contracts that are shared between the server and any agents
    public class SampleRequest : IRequest
    {
        public string Property1 { get; set; }
    }

    public class SampleRequest2 : IRequest
    {
        public string Property2 { get; set; }
    }
}
