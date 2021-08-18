using Microsoft.Fhir.Proxy.Configuration;

namespace Microsoft.Fhir.Proxy.Pipelines
{
    public class PipelineBuilder : IPipelineBuilder
    {
        public PipelineBuilder(string name, PipelineSettings settings)
        {
            this.name = name;
            this.settings = settings;
        }

        private readonly string name;
        private readonly PipelineSettings settings;

        public Pipeline Build()
        {
            return PipelineFactory.Create(name, settings);
        }

        public static implicit operator Pipeline(PipelineBuilder builder)
        {
            return PipelineFactory.Create(builder.name, builder.settings);
        }
    }
}
