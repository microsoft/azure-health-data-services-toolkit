using System.Collections;
using System.Collections.Generic;

namespace Fhir.Proxy.Channels
{
    public class OutputChannelCollection : IOutputChannelCollection
    {
        public OutputChannelCollection(IEnumerable<IOutputChannel> outputChannels = null)
        {
            channels = outputChannels != null ? new List<IChannel>(outputChannels) : new List<IChannel>();
        }

        private readonly List<IChannel> channels;

        public int Count => channels.Count;

        public bool IsReadOnly => false;

        public IChannel this[int index] { get => channels[index]; set => channels[index] = value; }

        public int IndexOf(IChannel item)
        {
            return channels.IndexOf(item);
        }

        public void Insert(int index, IChannel item)
        {
            channels.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            channels.RemoveAt(index);
        }

        public void Add(IChannel item)
        {
            channels.Add(item);
        }

        public void Clear()
        {
            channels.Clear();
        }

        public bool Contains(IChannel item)
        {
            return channels.Contains(item);
        }

        public void CopyTo(IChannel[] array, int arrayIndex)
        {
            channels.CopyTo(array, arrayIndex);
        }

        public bool Remove(IChannel item)
        {
            return channels.Remove(item);
        }

        public IEnumerator<IChannel> GetEnumerator()
        {
            return channels.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
