using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http;

namespace SimpleCustomOperation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomOperationController : ControllerBase
    {
        private readonly ILogger<CustomOperationController> _logger;
        private readonly IPipeline<HttpRequestMessage, HttpResponseMessage> _pipeline;

        public CustomOperationController(IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline, ILogger<CustomOperationController> logger = null)
        {
            _pipeline = pipeline;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string value)
        {
            _logger?.LogTrace("Value {val}", value);
            HttpRequestMessage request = Request.ConvertToHttpRequestMessage();
            HttpResponseMessage response = await _pipeline.ExecuteAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger?.LogTrace("Respone ok");
                return Ok(await response.Content.ReadAsStringAsync());
            }
            else
            {
                _logger?.LogTrace("Response bad request");
                return BadRequest(response);
            }

        }
    }
}