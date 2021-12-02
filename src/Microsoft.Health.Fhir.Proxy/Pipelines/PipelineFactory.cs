using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Configuration;
using Microsoft.Health.Fhir.Proxy.Filters;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    /// <summary>
    /// A factory of pipelines.
    /// </summary>
    public abstract class PipelineFactory
    {
        static PipelineFactory()
        {
            container = new Dictionary<string, Pipeline>();
        }

        private static readonly Dictionary<string, Pipeline> container;

        /// <summary>
        /// Gets an array of pipeline names.
        /// </summary>
        /// <returns>Array string names.</returns>
        public static string[] GetNames()
        {
            return container.Keys.ToArray();
        }

        /// <summary>
        /// Clears the pipeline factory.
        /// </summary>
        public static void Clear()
        {
            container.Clear();
        }


        /// <summary>
        /// Registers a pipeline in the factory.
        /// </summary>
        /// <param name="name">Pipeline name that matches the name property of the pipeline.</param>
        /// <param name="type">Type of pipeline.</param>
        /// <param name="args">Arguments used in the constructor of the pipeline type.</param>
        public static void Register(Pipeline pipeline)
        {
            if (container.ContainsKey(pipeline.Name))
            {
                container.Remove(pipeline.Name);
            }

            container.Add(pipeline.Name, pipeline);
        }

        /// <summary>
        /// Creates a pipeline from the factory.
        /// </summary>
        /// <param name="settings">Pipeline settings.</param>
        /// <returns>Pipeline</returns>
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

        /// <summary>
        /// Create a pipeline from the factory by its name.
        /// </summary>
        /// <param name="name">Name of the pipeline in the factory to create.</param>
        /// <returns>Pipeline</returns>
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
