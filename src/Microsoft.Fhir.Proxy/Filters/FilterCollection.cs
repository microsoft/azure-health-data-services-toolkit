using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Proxy.Filters
{
    public class FilterCollection : ICollection<IFilter>, IList<IFilter>
    {
        public FilterCollection()
        {
            filters = new List<IFilter>();
        }

        private readonly List<IFilter> filters;

        public int Count => filters.Count;

        public bool IsReadOnly => false;

        public IFilter this[int index] { get => filters[index]; set => filters[index] = value; }

        public int IndexOf(IFilter item)
        {
            return filters.IndexOf(item);
        }

        public void Insert(int index, IFilter item)
        {
            filters.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            filters.RemoveAt(index);
        }

        public void Add(IFilter item)
        {
            filters.Add(item);
        }

        public void Clear()
        {
            filters.Clear();
        }

        public bool Contains(IFilter item)
        {
            return filters.Contains(item);
        }

        public void CopyTo(IFilter[] array, int arrayIndex)
        {
            filters.CopyTo(array, arrayIndex);
        }

        public bool Remove(IFilter item)
        {
            return filters.Remove(item);
        }

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
