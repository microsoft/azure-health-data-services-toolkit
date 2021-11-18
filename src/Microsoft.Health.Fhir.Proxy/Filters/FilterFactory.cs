using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Health.Fhir.Proxy.Filters
{
    /// <summary>
    /// A factory of filters.
    /// </summary>
    public abstract class FilterFactory
    {
        static FilterFactory()
        {
            Container = new();
        }

        public static Dictionary<string, Tuple<Type, object[]>> Container { get; private set; }

        /// <summary>
        /// Registers a filter in the factory.
        /// </summary>
        /// <param name="name">Filter name that matches the name property of the filter.</param>
        /// <param name="type">Type of filter.</param>
        /// <param name="args">Arguments used in the constructor of the filter type.</param>
        public static void Register(string name, Type type, object?[]? args = null)
        {
            if (Container.ContainsKey(name))
            {
                Container.Remove(name);
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            Container.Add(name, new Tuple<Type, object?[]?>(type, args));
        }

        /// <summary>
        /// Gets an array of filter names.
        /// </summary>
        /// <returns>Array string names.</returns>
        public static string[] GetNames()
        {
            if (Container == null)
            {
                return null;
            }

            return Container.Keys.Count > 0 ? Container.Keys.ToArray() : null;
        }

        /// <summary>
        /// Clears the filter factory.
        /// </summary>
        public static void Clear()
        {
            if (Container != null)
            {
                Container.Clear();
            }
        }

        /// <summary>
        /// Create a filter from the factory by its name.
        /// </summary>
        /// <param name="name">Name of the filter in the factory to create.</param>
        /// <returns>IFilter</returns>
        public static IFilter Create(string name)
        {
            if (Container.ContainsKey(name))
            {
                var tuple = Container[name];
                return Activator.CreateInstance(tuple.Item1, tuple.Item2) as IFilter;
            }
            else
            {
                throw new KeyNotFoundException($"Filter name '{name}'not found.");
            }
        }
    }
}
