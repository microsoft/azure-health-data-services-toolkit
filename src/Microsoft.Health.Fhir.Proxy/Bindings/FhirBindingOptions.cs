namespace Microsoft.Health.Fhir.Proxy.Bindings
{
    /// <summary>
    /// Options for FHIR binding.
    /// </summary>
    public class FhirBindingOptions
    {

        private string routePrefix = "fhir";
        /// <summary>
        /// Gets or sets the URL of the FHIR server.
        /// </summary>
        public string FhirServerUrl { get; set; }

        /// <summary>
        /// Gets or sets the route prefix. The default is fhir.
        /// </summary>
        public string RoutePrefix
        {
            get { return routePrefix; }
            set { routePrefix = value; }
        }

    }
}
