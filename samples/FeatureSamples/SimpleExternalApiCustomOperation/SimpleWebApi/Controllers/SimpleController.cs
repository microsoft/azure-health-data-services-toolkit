using Microsoft.AspNetCore.Mvc;
using Shared;

namespace SimpleWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SimpleController : ControllerBase
    {
        private readonly ILogger<SimpleController> _logger;

        public SimpleController(ILogger<SimpleController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public Message Post(Message message)
        {
            _logger?.LogTrace("Simple Web API");
            return new Message() { Value = $"{message.Value}-WebApi" };
        }

        [HttpGet]
        public string Get()
        {
            return "test";
        }
    }
}