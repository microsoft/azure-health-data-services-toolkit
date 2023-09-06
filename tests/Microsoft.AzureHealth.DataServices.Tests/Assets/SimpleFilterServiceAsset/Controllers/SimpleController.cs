using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets.SimpleFilterServiceAsset.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SimpleController : ControllerBase
    {
        private readonly IPipeline<HttpRequestMessage, HttpResponseMessage> _pipeline;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public SimpleController(IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = null, ILogger<SimpleController> logger = null)
        {
            _pipeline = pipeline;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(TestMessage message)
        {
            // Replace is used to remove new lines from the log message per CodeQL security scanning.
            // https://cwe.mitre.org/data/definitions/117.html
            // https://owasp.org/www-community/attacks/Log_Injection
            _logger?.LogInformation("{info}", message.Value.Replace(Environment.NewLine, ""));

            HttpRequestMessage request = Request.ConvertToHttpRequestMessage();
            HttpResponseMessage response = await _pipeline.ExecuteAsync(request);
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
