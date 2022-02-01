using System;

namespace Fhir.Proxy.Extensions.Channels
{
    /// <summary>
    /// Event Hub SKU type.
    /// </summary>
    [Serializable]
    public enum EventHubSkuType
    {
        Basic,
        Standard,
        Premium,
        Dedicated
    }
}
