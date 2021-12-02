using Microsoft.Health.Fhir.Proxy.Configuration;

namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    /// <summary>
    /// Pipeline builder.
    /// </summary>
    public class PipelineBuilder : IPipelineBuilder
    {
        /// <summary>
        /// Creates an instance of PipelineBuilder.
        /// </summary>
        /// <param name="settings">Settings for the pipeline.</param>
        public PipelineBuilder(PipelineSettings settings)
        {
            this.settings = settings;
        }

        private readonly PipelineSettings settings;

        /// <summary>
        /// Builds and pipeline and returns it.
        /// </summary>
        /// <returns>Pipeline</returns>
        public Pipeline Build()
        {
            return PipelineFactory.Create(settings);
        }

        /// <summary>
        /// Creates the builder from the pipeline factory.
        /// </summary>
        /// <param name="builder">PipelineBuilder</param>
        public static implicit operator Pipeline(PipelineBuilder builder)
        {
            return PipelineFactory.Create(builder.settings);
        }
    }
}
