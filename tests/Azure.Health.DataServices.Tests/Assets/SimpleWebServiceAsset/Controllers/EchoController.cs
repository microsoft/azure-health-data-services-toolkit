using Microsoft.AspNetCore.Mvc;

namespace Azure.Health.DataServices.Tests.Assets.SimpleWebServiceAsset.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EchoController : ControllerBase
    {

        [HttpPost]
        public TestMessage Post(TestMessage message)
        {
            var customHeader1 = Request.Headers["X-MS-Test"];
            var customHeader2 = Request.Headers["X-MS-Identity"];
            return new TestMessage() { Value = $"{message.Value};WebApi;{customHeader1[0]};{customHeader2[0]}" };
        }
    }
}
