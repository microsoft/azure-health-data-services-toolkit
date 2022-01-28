using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Health.Fhir.Proxy.Pipelines;

namespace Microsoft.Health.Fhir.Proxy.Clients.Headers
{
    /// <summary>
    /// A collection of custom http header names that map to values associated with claims in a security token.
    /// </summary>
    public class HttpCustomIdentityHeaderCollection : IHttpCustomIdentityHeaderCollection
    {
        /// <summary>
        /// Creates an instance of HttpCustomIdentityHeaderCollection
        /// </summary>
        /// <param name="items">Items to initialize the collection.</param>
        public HttpCustomIdentityHeaderCollection(IEnumerable<IClaimValuePair> items)
        {
            list = new List<IClaimValuePair>(items);
        }

        private readonly IList<IClaimValuePair> list;

        /// <summary>
        /// Gets an item in the collection by its index.
        /// </summary>
        /// <param name="index">Index of item to return.</param>
        /// <returns>IClaimValuePair</returns>
        public IClaimValuePair this[int index] { get => list[index]; set => list[index] = value; }

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count => list.Count;

        /// <summary>
        /// Gets an indicator of whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Add an item to the collection.
        /// </summary>
        /// <param name="item"></param>
        public void Add(IClaimValuePair item)
        {
            list.Add(item);
        }

        /// <summary>
        /// Appends custom headers by matching claim types found in security token of the Authorization header of the request.
        /// </summary>
        /// <param name="request">Azure function request containing Authorization header.</param>
        /// <param name="headers">Existing header colleection.</param>
        /// <returns>Appended custom identity headers to the existing header collection.</returns>
        public NameValueCollection AppendCustomHeaders(HttpRequestData request, NameValueCollection? headers)
        {
            headers ??= new();
            var principal = request.GetClaimsPrincipal();
            var identity = principal.Identity;
            foreach (var item in list)
            {
                if (principal.HasClaim(claim => claim.Type == item.ClaimType))
                {
                    IEnumerable<Claim> claimset = principal.Claims.Where(claim => claim.Type == item.ClaimType);
                    foreach (var claim in claimset)
                    {
                        headers.Add(item.HeaderName, claim.Value);
                    }
                }
            }

            return headers;
        }

        /// <summary>
        /// Appends custom headers by matching claim types found in security token of the Authorization header of the request.
        /// </summary>
        /// <param name="request">Http request containing Authorization header.</param>
        /// <param name="headers">Existing header colleection.</param>
        /// <returns>Appended custom identity headers to the existing header collection.</returns>

        public NameValueCollection AppendCustomHeaders(HttpRequestMessage request, NameValueCollection headers)
        {
            var principal = request.GetClaimsPrincipal();
            var identity = principal.Identity;
            foreach (var item in list)
            {
                if (principal.HasClaim(claim => claim.Type == item.ClaimType))
                {
                    IEnumerable<Claim> claimset = principal.Claims.Where(claim => claim.Type == item.ClaimType);
                    foreach (var claim in claimset)
                    {
                        headers.Add(item.HeaderName, claim.Value);
                    }
                }
            }

            return headers;
        }


        /// <summary>
        /// Clears the collection.
        /// </summary>

        public void Clear()
        {
            list.Clear();
        }

        /// <summary>
        /// Indicates whether an item is contained in the collection.
        /// </summary>
        /// <param name="item">Item used to determined if it is in the collection.</param>
        /// <returns>True is item in is the collection; otherwise false.</returns>
        public bool Contains(IClaimValuePair item)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// Copies the collection into an array starting at the index.
        /// </summary>
        /// <param name="array">Array to fill with the collection.</param>
        /// <param name="arrayIndex">Starting index to fill the array.</param>
        public void CopyTo(IClaimValuePair[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets an enumerator for the collection.
        /// </summary>
        /// <returns>Enumerator of name value pairs.</returns>
        public IEnumerator<IClaimValuePair> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <summary>
        /// Finds the index of an item.
        /// </summary>
        /// <param name="item">The item to return the index.</param>
        /// <returns>Index of the item in the collection.</returns>
        public int IndexOf(IClaimValuePair item)
        {
            return list.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item into the collection.
        /// </summary>
        /// <param name="index">Index of the item insertion.</param>
        /// <param name="item">Item to insert.</param>
        public void Insert(int index, IClaimValuePair item)
        {
            list.Insert(index, item);
        }

        /// <summary>
        /// Removes an item from the collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <returns>True is the item is removed; otherwise false.</returns>
        public bool Remove(IClaimValuePair item)
        {
            return list.Remove(item);
        }

        /// <summary>
        /// Removes an item from the collection by its index.
        /// </summary>
        /// <param name="index">Index of item to remove.</param>
        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
