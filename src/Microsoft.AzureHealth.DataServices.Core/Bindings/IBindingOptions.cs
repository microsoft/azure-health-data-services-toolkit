namespace Microsoft.AzureHealth.DataServices.Bindings
{
    /// <summary>
    /// Common options for bindings that use HttpClient.
    /// </summary>
    public interface IBindingWithHttpClientOptions
    {
        /// <summary>
        /// BaseAddress for the HttpClient used by the binding.
        /// </summary>
        string BaseAddress { get; set; }
    }
}
