using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Server.Requests;

namespace  Server {
   [Route("api/[controller]")]
    public class TestController : Controller
    {
        private readonly IMediator mediator;

        public TestController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        // GET: api/values
        [HttpGet]
        public  async Task Get(string property1)
        {
            Console.WriteLine("Sending");
            await mediator.Send(new SampleRequest(){
              Property1 = property1,
            });
        }

        [HttpGet("get2")]
        public  async Task Get2(string property2)
        {
            Console.WriteLine("Sending");
            await mediator.Send(new SampleRequest2(){
              Property2 = property2,
            });
        }
    }
}