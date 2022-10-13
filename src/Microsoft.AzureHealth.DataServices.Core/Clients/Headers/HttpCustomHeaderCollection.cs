using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AzureHealth.DataServices.Pipelines;

namespace Microsoft.AzureHealth.DataServices.Clients.Headers
{
    /// <summary>
    /// A collection of custom http headers.
    /// </summary>
    public class HttpCustomHeaderCollection : IHttpCustomHeaderCollection
    {
        /// <summary>
        /// Creates an empty instance of HttpCustomHeaderCollection.
        /// </summary>
        public HttpCustomHeaderCollection() :
            this(new IHeaderNameValuePair[] { })
        {

        }

        /// <summary>
        /// Creates an instance of HttpCustomHeaderCollection.
        /// </summary>
        /// <param name="items">Items to initialize the collection.</param>
        public HttpCustomHeaderCollection(IEnumerable<IHeaderNameValuePair> items)
        {
            headers = new List<IHeaderNameValuePair>(items);
        }

        private readonly IList<IHeaderNameValuePair> headers;


        /// <summary>
        /// Gets an item in the collection by its index.
        /// </summary>
        /// <param name="index">Index of item to return.</param>
        /// <returns>INameValuePair</returns>
        public IHeaderNameValuePair this[int index] { get => headers[index]; set => headers[index] = value; }

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
        public void Add(IHeaderNameValuePair item)
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
            foreach (IHeaderNameValuePair nvp in headers)
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
        public bool Contains(IHeaderNameValuePair item)
        {
            return headers.Contains(item);
        }

        /// <summary>
        /// Copies the collection into an array starting at the index.
        /// </summary>
        /// <param name="array">Array to fill with the collection.</param>
        /// <param name="arrayIndex">Starting index to fill the array.</param>
        public void CopyTo(IHeaderNameValuePair[] array, int arrayIndex)
        {
            headers.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets an enumerator for the collection.
        /// </summary>
        /// <returns>Enumerator of name value pairs.</returns>
        public IEnumerator<IHeaderNameValuePair> GetEnumerator()
        {
            return headers.GetEnumerator();
        }

        /// <summary>
        /// Gets a collection of headers stored in the collection.
        /// </summary>
        /// <returns>Headers in the collection.</returns>
        public NameValueCollection GetHeaders()
        {
            NameValueCollection collection = new();
            foreach (IHeaderNameValuePair item in headers)
            {
                collection.Add(item.Name, item.Value);
            }

            return collection;
        }

        /// <summary>
        /// Appends and replaces existing request headers with custom headers and returns the modified collection headers.
        /// </summary>
        /// <param name="request">Http request message.</param>
        /// <returns>Modified collection headers</returns>
        public NameValueCollection RequestAppendAndReplace(HttpRequestMessage request)
        {
            NameValueCollection nvc = request.GetHeaders();

            foreach (IHeaderNameValuePair item in headers)
            {
                if (item.HeaderType == CustomHeaderType.RequestStatic)
                {
                    if (!string.IsNullOrEmpty(nvc[item.Name]))
                        nvc.Remove(item.Name);

                    nvc.Add(item.Name, item.Value);
                }

                if (item.HeaderType == CustomHeaderType.RequestIdentity)
                {
                    var principal = request.GetClaimsPrincipal();
                    if (principal is not null && principal.HasClaim(claim => claim.Type == item.Value))
                    {
                        IEnumerable<Claim> claimset = principal.Claims.Where(claim => claim.Type == item.Value);
                        foreach (var claim in claimset)
                        {
                            if (!string.IsNullOrEmpty(nvc[item.Name]))
                                nvc.Remove(item.Name);

                            nvc.Add(item.Name, claim.Value);
                        }
                    }
                }

                if (item.HeaderType == CustomHeaderType.RequestMatch)
                {
                    if (!string.IsNullOrEmpty(nvc[item.Name]))
                    {
                        var val = nvc[item.Name];
                        nvc.Add(item.Value, val);
                        nvc.Remove(item.Name);
                    }
                }
            }

            return nvc;
        }

        /// <summary>
        /// Updates this header collection from a HttpResponseMessage
        /// </summary>
        /// <param name="response">Http response message.</param>
        public void UpdateFromResponse(HttpResponseMessage response)
        {
            var responseHeaderEnumerator = response.Headers.GetEnumerator();
            while (responseHeaderEnumerator.MoveNext())
            {
                var current = responseHeaderEnumerator.Current;

                // If the current input element does not match a static response header in our existing collection
                if (!headers.Where(x => x.Name == current.Key && x.HeaderType == CustomHeaderType.ResponseStatic).Any())
                {
                    headers.Add(new HeaderNameValuePair(current.Key, current.Value.First(), CustomHeaderType.ResponseStatic));
                }
            }
        }

        /// <summary>
        /// Finds the index of an item.
        /// </summary>
        /// <param name="item">The item to return the index.</param>
        /// <returns>Index of the item in the collection.</returns>
        public int IndexOf(IHeaderNameValuePair item)
        {
            return headers.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item into the collection.
        /// </summary>
        /// <param name="index">Index of the item insertion.</param>
        /// <param name="item">Item to insert.</param>
        public void Insert(int index, IHeaderNameValuePair item)
        {
            headers.Insert(index, item);
        }

        /// <summary>
        /// Removes an item from the collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <returns>True is the item is removed; otherwise false.</returns>
        public bool Remove(IHeaderNameValuePair item)
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
