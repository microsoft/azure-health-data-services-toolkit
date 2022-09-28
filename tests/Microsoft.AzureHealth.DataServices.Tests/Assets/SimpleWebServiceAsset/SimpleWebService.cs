using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets.SimpleWebServiceAsset
{
    public class SimpleWebService
    {
        public SimpleWebService(int port)
        {
            this.port = port;
        }

        private readonly int port;
        private IHost host;

        public void Start()
        {
            host = CreateHostBuilder(null).Build();
            host.RunAsync(CancellationToken.None).GetAwaiter();
        }

        public void Stop()
        {
            host.StopAsync().GetAwaiter();
        }


        private IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHost(options =>
                {
                    options.UseStartup<SimpleWebServiceListener>();
                    options.UseKestrel();
                    options.ConfigureKestrel(options =>
                    {
                        options.ListenAnyIP(port);
                    });
                });
        }
    }
}
