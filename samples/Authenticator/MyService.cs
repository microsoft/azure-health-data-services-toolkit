using Azure.Health.DataServices.Security;
using Microsoft.Extensions.Options;

namespace AuthenticatorSample
{
    // This class is only fetching the token, but you would add more
    // code here for your custom operation.

    public class MyService : IMyService
    {
        public MyService(IOptions<MyServiceOptions> options, IAuthenticator authenticator)
        {
            fhirServerUrl = options.Value.FhirServerUrl;
            this.authenticator = authenticator;
        }

        private readonly string fhirServerUrl;
        private readonly IAuthenticator authenticator;

        public async Task<string> GetTokenAsync()
        {
            return await authenticator.AquireTokenForClientAsync(fhirServerUrl);
        }
    }
}
