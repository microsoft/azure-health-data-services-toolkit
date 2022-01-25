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
    public class HttpCustomIdentityHeaderCollection : IHttpCustomIdentityHeaderCollection
    {
        public HttpCustomIdentityHeaderCollection(IEnumerable<IClaimValuePair> items)
        {
            list = new List<IClaimValuePair>(items);
        }

        private readonly IList<IClaimValuePair> list;

        public IClaimValuePair this[int index] { get => list[index]; set => list[index] = value; }

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public void Add(IClaimValuePair item)
        {
            list.Add(item);
        }

        public NameValueCollection AppendCustomHeaders(HttpRequestData request, NameValueCollection headers)
        {
            var principal = request.GetClaimsPrincipal();
            var identity = principal.Identity;
            foreach(var item in list)
            {
                if(principal.HasClaim(claim => claim.Type == item.ClaimType))
                {
                    IEnumerable<Claim> claimset = principal.Claims.Where(claim => claim.Type == item.ClaimType);
                    foreach(var claim in claimset)
                    {
                        headers.Add(item.HeaderName, claim.Value);
                    }
                }
            }

            return headers;
        }

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





        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(IClaimValuePair item)
        {
            return list.Contains(item);
        }

        public void CopyTo(IClaimValuePair[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IClaimValuePair> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int IndexOf(IClaimValuePair item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, IClaimValuePair item)
        {
            list.Insert(index, item);
        }

        public bool Remove(IClaimValuePair item)
        {
            return list.Remove(item);
        }

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
