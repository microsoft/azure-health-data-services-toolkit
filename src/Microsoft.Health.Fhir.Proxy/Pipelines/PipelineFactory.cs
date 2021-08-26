using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Configuration;
using Microsoft.Health.Fhir.Proxy.Filters;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    public abstract class PipelineFactory
    {
        static PipelineFactory()
        {
            container = new Dictionary<string, Pipeline>();
        }

        private static readonly Dictionary<string, Pipeline> container;

        public static string[] GetNames()
        {
            return container.Keys.ToArray();
        }

        public static void Clear()
        {
            container.Clear();
        }

        public static void Register(Pipeline pipeline)
        {
            if (container.ContainsKey(pipeline.Name))
            {
                container.Remove(pipeline.Name);
            }

            container.Add(pipeline.Name, pipeline);
        }

        public static Pipeline Create(PipelineSettings settings)
        {
            FilterCollection filters = new();
            ChannelCollection channels = new();

            if (settings.FilterNames != null && settings.FilterNames.Count > 0)
            {
                foreach (var filterName in settings.FilterNames)
                {
                    filters.Add(FilterFactory.Create(filterName));
                }
            }

            if (settings.ChannelNames != null && settings.ChannelNames.Count > 0)
            {
                foreach (var channelName in settings.ChannelNames)
                {
                    channels.Add(ChannelFactory.Create(channelName));
                }
            }

            Pipeline pipeline = Create(settings.Name);
            pipeline.Filters = filters;
            pipeline.Channels = channels;

            return pipeline;
        }

        public static Pipeline Create(string name)
        {
            if (container.ContainsKey(name))
            {
                return container[name];
            }
            else
            {
                throw new KeyNotFoundException($"Pipeline name not found.");
            }
        }
    }
}
