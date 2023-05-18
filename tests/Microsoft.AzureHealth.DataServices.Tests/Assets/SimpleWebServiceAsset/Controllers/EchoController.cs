using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets.SimpleWebServiceAsset.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EchoController : ControllerBase
    {
        [HttpPost]
        public TestMessage Post(TestMessage message)
        {
            Extensions.Primitives.StringValues customHeader1 = Request.Headers["X-MS-Test"];
            Extensions.Primitives.StringValues customHeader2 = Request.Headers["X-MS-Identity"];
            return new TestMessage() { Value = $"{message.Value};WebApi;{customHeader1[0]};{customHeader2[0]}" };
        }
    }
}
