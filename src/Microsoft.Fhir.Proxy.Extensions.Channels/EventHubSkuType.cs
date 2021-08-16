using System;

namespace Microsoft.Fhir.Proxy.Extensions.Channels
{
    [Serializable]
    public enum EventHubSkuType
    {
        Basic,
        Standard,
        Premium,
        Dedicated
    }
}
