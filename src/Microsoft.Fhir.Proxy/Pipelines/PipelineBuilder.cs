using Microsoft.Fhir.Proxy.Configuration;

namespace Microsoft.Fhir.Proxy.Pipelines
{
    public class PipelineBuilder : IPipelineBuilder
    {
        public PipelineBuilder(PipelineSettings settings)
        {
            this.settings = settings;
        }

        private readonly PipelineSettings settings;

        public Pipeline Build()
        {
            return PipelineFactory.Create(settings);
        }

        public static implicit operator Pipeline(PipelineBuilder builder)
        {
            return PipelineFactory.Create(builder.settings);
        }
    }
}
