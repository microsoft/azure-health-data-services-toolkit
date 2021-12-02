using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Health.Fhir.Proxy.Channels
{
    /// <summary>
    /// A factory of channels.
    /// </summary>
    public abstract class ChannelFactory
    {
        static ChannelFactory()
        {
            container = new();
        }

        private static readonly Dictionary<string, Tuple<Type, object?[]?>> container;

        /// <summary>
        /// Registers a channel in the factory.
        /// </summary>
        /// <param name="name">Channel name that matches the name property of the channel.</param>
        /// <param name="type">Type of channel.</param>
        /// <param name="args">Arguments used in the constructor of the channel type.</param>
        public static void Register(string name, Type type, object?[]? args)
        {
            if (container.ContainsKey(name))
            {
                container.Remove(name);
            }

            container.Add(name, new Tuple<Type, object?[]?>(type, args));
        }

        /// <summary>
        /// Gets an array of channel names.
        /// </summary>
        /// <returns>Array string names.</returns>
        public static string[] GetNames()
        {
            if (container == null)
            {
                return null;
            }

            return container.Keys.Count > 0 ? container.Keys.ToArray() : null;
        }

        /// <summary>
        /// Clears the channel factory.
        /// </summary>
        public static void Clear()
        {
            if (container != null)
            {
                container.Clear();
            }
        }

        /// <summary>
        /// Create a channel from the factory by its name.
        /// </summary>
        /// <param name="name">Name of the channel in the factory to create.</param>
        /// <returns>IChannel</returns>
        public static IChannel Create(string name)
        {
            if (container.ContainsKey(name))
            {
                var tuple = container[name];
                return Activator.CreateInstance(tuple.Item1, tuple.Item2) as IChannel;
            }
            else
            {
                throw new KeyNotFoundException($"Channel name not found.");
            }
        }

    }
}
