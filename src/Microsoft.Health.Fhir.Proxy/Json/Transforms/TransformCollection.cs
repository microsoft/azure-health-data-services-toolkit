using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Proxy.Json.Transforms
{
    public class TransformCollection : ICollection<Transform>, IList<Transform>
    {
        public TransformCollection()
        {
            transforms = new List<Transform>();
        }

        private readonly List<Transform> transforms;

        public int Count => transforms.Count;

        public bool IsReadOnly => false;

        public Transform this[int index] { get => transforms[index]; set => transforms[index] = value; }

        public int IndexOf(Transform item)
        {
            return transforms.IndexOf(item);
        }

        public void Insert(int index, Transform item)
        {
            transforms.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            transforms.RemoveAt(index);
        }

        public void Add(Transform item)
        {
            transforms.Add(item);
        }

        public void Clear()
        {
            transforms.Clear();
        }

        public bool Contains(Transform item)
        {
            return transforms.Contains(item);
        }

        public void CopyTo(Transform[] array, int arrayIndex)
        {
            transforms.CopyTo(array, arrayIndex);
        }

        public bool Remove(Transform item)
        {
            return transforms.Remove(item);
        }

        public IEnumerator<Transform> GetEnumerator()
        {
            return transforms.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return transforms.GetEnumerator();
        }
    }
}
