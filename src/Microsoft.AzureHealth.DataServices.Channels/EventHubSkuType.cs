using System;

namespace Microsoft.AzureHealth.DataServices.Channels
{
    /// <summary>
    /// Event Hub SKU type.
    /// </summary>
    [Serializable]
    public enum EventHubSkuType
    {
        /// <summary>
        /// Basic SKU
        /// </summary>
        Basic,

        /// <summary>
        /// Standard SKU
        /// </summary>
        Standard,

        /// <summary>
        /// Premium SKU
        /// </summary>
        Premium,

        /// <summary>
        /// Dedicated SKU
        /// </summary>
        Dedicated,
    }
}
