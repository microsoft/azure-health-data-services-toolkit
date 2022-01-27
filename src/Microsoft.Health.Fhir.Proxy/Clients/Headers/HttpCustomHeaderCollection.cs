using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Microsoft.Health.Fhir.Proxy.Clients.Headers
{
    public class HttpCustomHeaderCollection : IHttpCustomHeaderCollection
    {
        public HttpCustomHeaderCollection(IEnumerable<INameValuePair> items)
        {
            headers = new List<INameValuePair>(items);
        }

        private readonly IList<INameValuePair> headers;


        public INameValuePair this[int index] { get => headers[index]; set => headers[index] = value; }

        public int Count => headers.Count;

        public bool IsReadOnly => false;

        public void Add(INameValuePair item)
        {
            headers.Add(item);
        }

        public NameValueCollection AppendHeaders(NameValueCollection items)
        {
            foreach (INameValuePair nvp in headers)
            {
                items.Add(nvp.Name, nvp.Value);
            }

            return items;
        }

        public void Clear()
        {
            headers.Clear();
        }

        public bool Contains(INameValuePair item)
        {
            return headers.Contains(item);
        }

        public void CopyTo(INameValuePair[] array, int arrayIndex)
        {
            headers.CopyTo(array, arrayIndex);
        }

        public IEnumerator<INameValuePair> GetEnumerator()
        {
            return headers.GetEnumerator();
        }

        public NameValueCollection GetHeaders()
        {
            NameValueCollection collection = new NameValueCollection();
            foreach (INameValuePair item in headers)
            {
                collection.Add(item.Name, item.Value);
            }

            return collection;
        }

        public int IndexOf(INameValuePair item)
        {
            return headers.IndexOf(item);
        }

        public void Insert(int index, INameValuePair item)
        {
            headers.Insert(index, item);
        }

        public bool Remove(INameValuePair item)
        {
            return headers.Remove(item);
        }

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
