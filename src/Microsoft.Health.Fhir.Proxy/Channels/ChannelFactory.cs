using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Health.Fhir.Proxy.Channels
{
    public abstract class ChannelFactory
    {
        static ChannelFactory()
        {
            container = new();
        }

        private static readonly Dictionary<string, Tuple<Type, object?[]?>> container;


        public static void Register(string name, Type type, object?[]? args)
        {
            if (container.ContainsKey(name))
            {
                container.Remove(name);
            }

            container.Add(name, new Tuple<Type, object?[]?>(type, args));
        }

        public static string[] GetNames()
        {
            if (container == null)
            {
                return null;
            }

            return container.Keys.Count > 0 ? container.Keys.ToArray() : null;
        }

        public static void Clear()
        {
            if (container != null)
            {
                container.Clear();
            }
        }

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
