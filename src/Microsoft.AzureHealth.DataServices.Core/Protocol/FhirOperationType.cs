using System.ComponentModel;

namespace Microsoft.AzureHealth.DataServices.Protocol
{
    /// <summary>
    /// FHIR operation type enum.
    /// </summary>
    public enum FhirOperationType
    {
        /// <summary>
        /// FHIR $reindex operation
        /// </summary>
        [Description("$reindex"), Category("async")]
        Reindex,

        /// <summary>
        /// FHIR $convert-data operation
        /// </summary>
        [Description("$convert-data")]
        ConvertData,

        /// <summary>
        /// FHIR $import operation
        /// </summary>
        [Description("$import"), Category("async")]
        Import,

        /// <summary>
        /// FHIR $import operation
        /// </summary>
        [Description("$export"), Category("async")]
        Export
    }
}
