using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Proxy.Filters
{
    public class OutputFilterCollection : IOutputFilterCollection
    {

        /// <summary>
        /// Creates an instance of the FilterCollection.
        /// </summary>
        public OutputFilterCollection(IEnumerable<IInputFilter> outputFilters = null)
        {
            filters = outputFilters != null ? new List<IFilter>(outputFilters) : new List<IFilter>();
        }

        private readonly List<IFilter> filters;

        /// <summary>
        /// Gets the number of filters in the collection.
        /// </summary>
        public int Count => filters.Count;

        /// <summary>
        /// Gets an indicator of whether the filter is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets a filter in the collection by its index.
        /// </summary>
        /// <param name="index">Index of filter to return.</param>
        /// <returns>IFilter</returns>
        public IFilter this[int index] { get => filters[index]; set => filters[index] = value; }

        /// <summary>
        /// Finds the index of a filter.
        /// </summary>
        /// <param name="item">The filter to return the index.</param>
        /// <returns>Index of the input filter in the collection.</returns>
        public int IndexOf(IFilter item)
        {
            return filters.IndexOf(item);
        }

        /// <summary>
        /// Inserts a filter into the collection.
        /// </summary>
        /// <param name="index">Index of the filter insertion.</param>
        /// <param name="item">Fitler to insert.</param>
        public void Insert(int index, IFilter item)
        {
            filters.Insert(index, item);
        }

        /// <summary>
        /// Remove a filter from the collection by its index.
        /// </summary>
        /// <param name="index">Index of filter to remove.</param>
        public void RemoveAt(int index)
        {
            filters.RemoveAt(index);
        }

        /// <summary>
        /// Adds a filter to the collection.
        /// </summary>
        /// <param name="item">Filter to add to the collection.</param>
        public void Add(IFilter item)
        {
            filters.Add(item);
        }

        /// <summary>
        /// Clears the filter collections.
        /// </summary>
        public void Clear()
        {
            filters.Clear();
        }

        /// <summary>
        /// Indicates whether a filter is contained in the collection.
        /// </summary>
        /// <param name="item">Fitler used to determined if it is in the collection.</param>
        /// <returns>True is filter in is the collection; otherwise false.</returns>
        public bool Contains(IFilter item)
        {
            return filters.Contains(item);
        }

        /// <summary>
        /// Copies the filter collection into an array starting at the index.
        /// </summary>
        /// <param name="array">Array to fill with filter collection.</param>
        /// <param name="arrayIndex">Starting index to fill the array.</param>
        public void CopyTo(IFilter[] array, int arrayIndex)
        {
            filters.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes a filter from the collection.
        /// </summary>
        /// <param name="item">Filter to remove.</param>
        /// <returns>True is the filter is removed; otherwise false.</returns>
        public bool Remove(IFilter item)
        {
            return filters.Remove(item);
        }

        /// <summary>
        /// Gets an enumerator for the filters in the collection.
        /// </summary>
        /// <returns>Enumerator of filters.</returns>
        public IEnumerator<IFilter> GetEnumerator()
        {
            return filters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
