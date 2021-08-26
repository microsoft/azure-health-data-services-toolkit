using System;

namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels
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
