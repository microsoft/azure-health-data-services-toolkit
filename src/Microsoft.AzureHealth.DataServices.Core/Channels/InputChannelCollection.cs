using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AzureHealth.DataServices.Channels
{
    /// <summary>
    /// An input channel collection.
    /// </summary>
    public class InputChannelCollection : IInputChannelCollection
    {
        /// <summary>
        /// Creates an instance of InputChannelCollection.
        /// </summary>
        /// <param name="inputChannels">Optional collection of input channels to initialize.</param>
        public InputChannelCollection(IEnumerable<IInputChannel> inputChannels = null)
        {
            channels = inputChannels != null ? new List<IChannel>(inputChannels) : new List<IChannel>();
        }

        private readonly List<IChannel> channels;

        /// <summary>
        /// Gets the count of channels in the collection.
        /// </summary>
        public int Count => channels.Count;

        /// <summary>
        /// Gets an indicator whether the channel is readonly.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets a channel by its index.
        /// </summary>
        /// <param name="index">Index of channel</param>
        /// <returns>IChannel</returns>
        public IChannel this[int index] { get => channels[index]; set => channels[index] = value; }

        /// <summary>
        /// Gets the index of a channel.
        /// </summary>
        /// <param name="item">Channel get index.</param>
        /// <returns>Index of channel.</returns>
        public int IndexOf(IChannel item)
        {
            return channels.IndexOf(item);
        }

        /// <summary>
        /// Inserts a channel into the collection.
        /// </summary>
        /// <param name="index">Index of channel to insert.</param>
        /// <param name="item">Channel to insert.</param>
        public void Insert(int index, IChannel item)
        {
            channels.Insert(index, item);
        }

        /// <summary>
        /// Removes channel by its index.
        /// </summary>
        /// <param name="index">Index of channel to remove.</param>
        public void RemoveAt(int index)
        {
            channels.RemoveAt(index);
        }

        /// <summary>
        /// Adds a channel to the collection.
        /// </summary>
        /// <param name="item">Channel to add.</param>
        public void Add(IChannel item)
        {
            channels.Add(item);
        }

        /// <summary>
        /// Clears the channel collection.
        /// </summary>
        public void Clear()
        {
            channels.Clear();
        }

        /// <summary>
        /// Gets an indicator as to whether the channel is contained in the collection.
        /// </summary>
        /// <param name="item">Channel to test.</param>
        /// <returns>True if channel is contained in the collection; otherwise false.</returns>
        public bool Contains(IChannel item)
        {
            return channels.Contains(item);
        }

        /// <summary>
        /// Copies the channel collection into an array.
        /// </summary>
        /// <param name="array">Array to copy into.</param>
        /// <param name="arrayIndex">Starting index of the array.</param>
        public void CopyTo(IChannel[] array, int arrayIndex)
        {
            channels.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes a channel from the collection.
        /// </summary>
        /// <param name="item">Channel to remove.</param>
        /// <returns>True is channel is removed; otherwise false.</returns>
        public bool Remove(IChannel item)
        {
            return channels.Remove(item);
        }

        /// <summary>
        /// Gets an enumerator of the channel collection.
        /// </summary>
        /// <returns>Enumerator.</returns>
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
