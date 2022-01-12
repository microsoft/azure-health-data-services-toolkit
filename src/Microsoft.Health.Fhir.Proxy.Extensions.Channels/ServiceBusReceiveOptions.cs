using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels
{
    public class ServiceBusReceiveOptions
    {
        public string ConnectionString { get; set; }

        public string Topic { get; set; }

        public string Subscription { get; set; }

        public string FallbackStorageConnectionString { get; set; }


    }
}
