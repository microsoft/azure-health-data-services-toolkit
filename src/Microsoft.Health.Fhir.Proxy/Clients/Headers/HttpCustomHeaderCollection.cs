using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Microsoft.Health.Fhir.Proxy.Clients.Headers
{
    /// <summary>
    /// A collection of custom http headers.
    /// </summary>
    public class HttpCustomHeaderCollection : IHttpCustomHeaderCollection
    {
        /// <summary>
        /// Creates an instance of HttpCustomHeaderCollection.
        /// </summary>
        /// <param name="items">Items to initialize the collection.</param>
        public HttpCustomHeaderCollection(IEnumerable<INameValuePair> items)
        {
            headers = new List<INameValuePair>(items);
        }

        private readonly IList<INameValuePair> headers;


        /// <summary>
        /// Gets an item in the collection by its index.
        /// </summary>
        /// <param name="index">Index of item to return.</param>
        /// <returns>INameValuePair</returns>
        public INameValuePair this[int index] { get => headers[index]; set => headers[index] = value; }

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count => headers.Count;

        /// <summary>
        /// Gets an indicator of whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Add an item to the collection.
        /// </summary>
        /// <param name="item"></param>
        public void Add(INameValuePair item)
        {
            headers.Add(item);
        }

        /// <summary>
        /// Appends headers to an existing collection of headers and returns results.
        /// </summary>
        /// <param name="items">Existing header collection.</param>
        /// <returns>The existing headers with the headers in the collection appended.</returns>
        public NameValueCollection AppendHeaders(NameValueCollection items)
        {
            foreach (INameValuePair nvp in headers)
            {
                items.Add(nvp.Name, nvp.Value);
            }

            return items;
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            headers.Clear();
        }

        /// <summary>
        /// Indicates whether an item is contained in the collection.
        /// </summary>
        /// <param name="item">Item used to determined if it is in the collection.</param>
        /// <returns>True is item in is the collection; otherwise false.</returns>
        public bool Contains(INameValuePair item)
        {
            return headers.Contains(item);
        }

        /// <summary>
        /// Copies the collection into an array starting at the index.
        /// </summary>
        /// <param name="array">Array to fill with the collection.</param>
        /// <param name="arrayIndex">Starting index to fill the array.</param>
        public void CopyTo(INameValuePair[] array, int arrayIndex)
        {
            headers.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets an enumerator for the collection.
        /// </summary>
        /// <returns>Enumerator of name value pairs.</returns>
        public IEnumerator<INameValuePair> GetEnumerator()
        {
            return headers.GetEnumerator();
        }

        /// <summary>
        /// Gets a collection of headers stored in the collection.
        /// </summary>
        /// <returns>Headers in the collection.</returns>
        public NameValueCollection GetHeaders()
        {
            NameValueCollection collection = new NameValueCollection();
            foreach (INameValuePair item in headers)
            {
                collection.Add(item.Name, item.Value);
            }

            return collection;
        }

        /// <summary>
        /// Finds the index of an item.
        /// </summary>
        /// <param name="item">The item to return the index.</param>
        /// <returns>Index of the item in the collection.</returns>
        public int IndexOf(INameValuePair item)
        {
            return headers.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item into the collection.
        /// </summary>
        /// <param name="index">Index of the item insertion.</param>
        /// <param name="item">Item to insert.</param>
        public void Insert(int index, INameValuePair item)
        {
            headers.Insert(index, item);
        }

        /// <summary>
        /// Removes an item from the collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <returns>True is the item is removed; otherwise false.</returns>
        public bool Remove(INameValuePair item)
        {
            return headers.Remove(item);
        }

        /// <summary>
        /// Removes an item from the collection by its index.
        /// </summary>
        /// <param name="index">Index of item to remove.</param>
        public void RemoveAt(int index)
        {
            headers.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
