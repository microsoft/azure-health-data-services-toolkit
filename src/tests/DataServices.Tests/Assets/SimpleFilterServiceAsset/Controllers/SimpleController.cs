using System.Net.Http;
using System.Threading.Tasks;
using DataServices.Pipelines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DataServices.Tests.Assets.SimpleFilterServiceAsset.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class SimpleController : ControllerBase
    {
        public SimpleController(IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = null, ILogger<SimpleController> logger = null)
        {
            this.pipeline = pipeline;
            this.logger = logger;
        }

        private readonly IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline;
        private readonly Microsoft.Extensions.Logging.ILogger logger;

        [HttpPost]
        public async Task<IActionResult> Post(TestMessage message)
        {
            logger?.LogInformation("{info}", message.Value);

            HttpRequestMessage request = Request.ConvertToHttpRequestMesssage();
            HttpResponseMessage response = await pipeline.ExecuteAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                TestMessage testMessage = JsonConvert.DeserializeObject<TestMessage>(content);
                return Ok(testMessage);
            }
            else
            {
                return BadRequest(response);
            }
        }

        [HttpGet]
        public IActionResult Get(string value)
        {
            TestMessage message = new() { Value = value };
            return Ok(message);
        }
    }
}
