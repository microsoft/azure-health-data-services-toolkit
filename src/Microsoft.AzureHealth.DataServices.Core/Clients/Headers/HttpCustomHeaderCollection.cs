using System;
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
        private readonly IList<IHeaderNameValuePair> _headers;

        /// <summary>
        /// Creates an empty instance of HttpCustomHeaderCollection.
        /// </summary>
        public HttpCustomHeaderCollection()
            : this(Array.Empty<IHeaderNameValuePair>())
        {
        }

        /// <summary>
        /// Creates an instance of HttpCustomHeaderCollection.
        /// </summary>
        /// <param name="items">Items to initialize the collection.</param>
        public HttpCustomHeaderCollection(IEnumerable<IHeaderNameValuePair> items)
        {
            _headers = new List<IHeaderNameValuePair>(items);
        }

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count => _headers.Count;

        /// <summary>
        /// Gets an indicator of whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets an item in the collection by its index.
        /// </summary>
        /// <param name="index">Index of item to return.</param>
        /// <returns>INameValuePair</returns>
        public IHeaderNameValuePair this[int index] { get => _headers[index]; set => _headers[index] = value; }

        /// <summary>
        /// Add an item to the collection.
        /// </summary>
        /// <param name="item">Header item.</param>
        public void Add(IHeaderNameValuePair item)
        {
            _headers.Add(item);
        }

        /// <summary>
        /// Appends headers to an existing collection of headers and returns results.
        /// </summary>
        /// <param name="items">Existing header collection.</param>
        /// <returns>The existing headers with the headers in the collection appended.</returns>
        public NameValueCollection AppendHeaders(NameValueCollection items)
        {
            foreach (IHeaderNameValuePair nvp in _headers)
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
            _headers.Clear();
        }

        /// <summary>
        /// Indicates whether an item is contained in the collection.
        /// </summary>
        /// <param name="item">Item used to determined if it is in the collection.</param>
        /// <returns>True is item in is the collection; otherwise false.</returns>
        public bool Contains(IHeaderNameValuePair item)
        {
            return _headers.Contains(item);
        }

        /// <summary>
        /// Copies the collection into an array starting at the index.
        /// </summary>
        /// <param name="array">Array to fill with the collection.</param>
        /// <param name="arrayIndex">Starting index to fill the array.</param>
        public void CopyTo(IHeaderNameValuePair[] array, int arrayIndex)
        {
            _headers.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets an enumerator for the collection.
        /// </summary>
        /// <returns>Enumerator of name value pairs.</returns>
        public IEnumerator<IHeaderNameValuePair> GetEnumerator()
        {
            return _headers.GetEnumerator();
        }

        /// <summary>
        /// Gets a collection of headers stored in the collection.
        /// </summary>
        /// <returns>Headers in the collection.</returns>
        public NameValueCollection GetHeaders()
        {
            NameValueCollection collection = new();
            foreach (IHeaderNameValuePair item in _headers)
            {
                collection.Add(item.Name, item.Value);
            }

            return collection;
        }

        /// <summary>
        /// Appends and replaces existing request headers with custom headers and returns the modified collection headers.
        /// </summary>
        /// <param name="request">Http request message.</param>
        /// <param name="restricted">Restrict to user editable headers?</param>
        /// <returns>Modified collection headers</returns>
        public NameValueCollection RequestAppendAndReplace(HttpRequestMessage request, bool restricted = true)
        {
            NameValueCollection nvc = request.GetHeaders(restricted);

            foreach (IHeaderNameValuePair item in _headers)
            {
                if (item.HeaderType == CustomHeaderType.RequestStatic)
                {
                    if (!string.IsNullOrEmpty(nvc[item.Name]))
                    {
                        nvc.Remove(item.Name);
                    }

                    nvc.Add(item.Name, item.Value);
                }

                if (item.HeaderType == CustomHeaderType.RequestIdentity)
                {
                    ClaimsPrincipal principal = request.GetClaimsPrincipal();
                    if (principal is not null && principal.HasClaim(claim => claim.Type == item.Value))
                    {
                        IEnumerable<Claim> claimset = principal.Claims.Where(claim => claim.Type == item.Value);
                        foreach (Claim claim in claimset)
                        {
                            if (!string.IsNullOrEmpty(nvc[item.Name]))
                            {
                                nvc.Remove(item.Name);
                            }

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
        /// <param name="restricted">If true (default), omits the following headers, Content-Length, Authorization, Transfer-Encoding.  Otherwise, returns all headers. </param>
        public void UpdateFromResponse(HttpResponseMessage response, bool restricted = true)
        {
            NameValueCollection nvc = response.GetHeaders(restricted);

            foreach (var key in nvc.AllKeys)
            {
                // If the current input element does not match a static response header in our existing collection
                if (!_headers.Where(x => x.Name == key && x.HeaderType == CustomHeaderType.ResponseStatic).Any())
                {
                    _headers.Add(new HeaderNameValuePair(key, nvc[key], CustomHeaderType.ResponseStatic));
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
            return _headers.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item into the collection.
        /// </summary>
        /// <param name="index">Index of the item insertion.</param>
        /// <param name="item">Item to insert.</param>
        public void Insert(int index, IHeaderNameValuePair item)
        {
            _headers.Insert(index, item);
        }

        /// <summary>
        /// Removes an item from the collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <returns>True is the item is removed; otherwise false.</returns>
        public bool Remove(IHeaderNameValuePair item)
        {
            return _headers.Remove(item);
        }

        /// <summary>
        /// Removes an item from the collection by its index.
        /// </summary>
        /// <param name="index">Index of item to remove.</param>
        public void RemoveAt(int index)
        {
            _headers.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
