using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Health.Fhir.Proxy.Filters
{
    public abstract class FilterFactory
    {
        static FilterFactory()
        {
            Container = new();
        }

        public static Dictionary<string, Tuple<Type, object[]>> Container { get; private set; }

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

        public static string[] GetNames()
        {
            if (Container == null)
            {
                return null;
            }

            return Container.Keys.Count > 0 ? Container.Keys.ToArray() : null;
        }

        public static void Clear()
        {
            if (Container != null)
            {
                Container.Clear();
            }
        }

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
