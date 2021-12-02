using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Proxy.Channels
{
    /// <summary>
    /// A collection of channels.
    /// </summary>
    public class ChannelCollection : ICollection<IChannel>, IList<IChannel>
    {
        /// <summary>
        /// Creates an instance of the channel collection.
        /// </summary>
        public ChannelCollection()
        {
            channels = new List<IChannel>();
        }

        private readonly List<IChannel> channels;

        /// <summary>
        /// Gets the number of channels in the collection.
        /// </summary>
        public int Count => channels.Count;

        /// <summary>
        /// Gets an indicator of whether the channel is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets a channel in the collection by its index.
        /// </summary>
        /// <param name="index">Index of channel to return.</param>
        /// <returns>IChannel</returns>
        public IChannel this[int index] { get => channels[index]; set => channels[index] = value; }

        /// <summary>
        /// Finds the index of a channel.
        /// </summary>
        /// <param name="item">The channel to return the index.</param>
        /// <returns>Index of the input channel in the collection.</returns>
        public int IndexOf(IChannel item)
        {
            return channels.IndexOf(item);
        }

        /// <summary>
        /// Inserts a channel into the collection.
        /// </summary>
        /// <param name="index">Index of the channel insertion.</param>
        /// <param name="item">Channel to insert.</param>
        public void Insert(int index, IChannel item)
        {
            channels.Insert(index, item);
        }

        /// <summary>
        /// Remove a channel from the collection by its index.
        /// </summary>
        /// <param name="index">Index of channel to remove.</param>
        public void RemoveAt(int index)
        {
            channels.RemoveAt(index);
        }

        /// <summary>
        /// Adds a channel to the collection.
        /// </summary>
        /// <param name="item">Channel to add to the collection.</param>
        public void Add(IChannel item)
        {
            channels.Add(item);
        }

        /// <summary>
        /// Clears the channel collections.
        /// </summary>
        public void Clear()
        {
            channels.Clear();
        }

        /// <summary>
        /// Indicates whether a channel is contained in the collection.
        /// </summary>
        /// <param name="item">Channel used to determined if it is in the collection.</param>
        /// <returns>True is channel in is the collection; otherwise false.</returns>
        public bool Contains(IChannel item)
        {
            return channels.Contains(item);
        }

        /// <summary>
        /// Copies the channel collection into an array starting at the index.
        /// </summary>
        /// <param name="array">Array to fill with channel collection.</param>
        /// <param name="arrayIndex">Starting index to fill the array.</param>
        public void CopyTo(IChannel[] array, int arrayIndex)
        {
            channels.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes a channel from the collection.
        /// </summary>
        /// <param name="item">Channel to remove.</param>
        /// <returns>True is the channel is removed; otherwise false.</returns>
        public bool Remove(IChannel item)
        {
            return channels.Remove(item);
        }

        /// <summary>
        /// Gets an enumerator for the channels in the collection.
        /// </summary>
        /// <returns>Enumerator of channels.</returns>
        public IEnumerator<IChannel> GetEnumerator()
        {
            return channels.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return channels.GetEnumerator();
        }
    }
}
