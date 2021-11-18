namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    /// <summary>
    /// Interface for pipeline builder.
    /// </summary>
    public interface IPipelineBuilder
    {
        Pipeline Build();
    }
}
