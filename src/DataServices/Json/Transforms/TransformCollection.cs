using System.Collections;
using System.Collections.Generic;

namespace Azure.Health.DataServices.Json.Transforms
{
    /// <summary>
    /// A collection of transforms.
    /// </summary>
    public class TransformCollection : ICollection<Transform>, IList<Transform>
    {
        /// <summary>
        /// Creates an instance of the transform collection.
        /// </summary>
        public TransformCollection()
        {
            transforms = new List<Transform>();
        }

        private readonly List<Transform> transforms;

        /// <summary>
        /// Gets the number of transforms in the collection.
        /// </summary>
        public int Count => transforms.Count;

        /// <summary>
        /// Gets an indicator of whether the transform is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets a transform in the collection by its index.
        /// </summary>
        /// <param name="index">Index of transform to return.</param>
        /// <returns>Transform</returns>
        public Transform this[int index] { get => transforms[index]; set => transforms[index] = value; }

        /// <summary>
        /// Finds the index of a transform.
        /// </summary>
        /// <param name="item">The transform to return the index.</param>
        /// <returns>Index of the input transform in the collection.</returns>
        public int IndexOf(Transform item)
        {
            return transforms.IndexOf(item);
        }

        /// <summary>
        /// Inserts a transform into the collection.
        /// </summary>
        /// <param name="index">Index of the transform insertion.</param>
        /// <param name="item">Transform to insert.</param>
        public void Insert(int index, Transform item)
        {
            transforms.Insert(index, item);
        }

        /// <summary>
        /// Remove a transform from the collection by its index.
        /// </summary>
        /// <param name="index">Index of transform to remove.</param>
        public void RemoveAt(int index)
        {
            transforms.RemoveAt(index);
        }

        /// <summary>
        /// Adds a transform to the collection.
        /// </summary>
        /// <param name="item">Transform to add to the collection.</param>
        public void Add(Transform item)
        {
            transforms.Add(item);
        }

        /// <summary>
        /// Clears the transform collections.
        /// </summary>
        public void Clear()
        {
            transforms.Clear();
        }

        /// <summary>
        /// Indicates whether a transform is contained in the collection.
        /// </summary>
        /// <param name="item">Transform used to determined if it is in the collection.</param>
        /// <returns>True is transform in is the collection; otherwise false.</returns>
        public bool Contains(Transform item)
        {
            return transforms.Contains(item);
        }

        /// <summary>
        /// Copies the transform collection into an array starting at the index.
        /// </summary>
        /// <param name="array">Array to fill with transform collection.</param>
        /// <param name="arrayIndex">Starting index to fill the array.</param>
        public void CopyTo(Transform[] array, int arrayIndex)
        {
            transforms.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes a transform from the collection.
        /// </summary>
        /// <param name="item">Transform to remove.</param>
        /// <returns>True is the transform is removed; otherwise false.</returns>
        public bool Remove(Transform item)
        {
            return transforms.Remove(item);
        }

        /// <summary>
        /// Gets an enumerator for the transforms in the collection.
        /// </summary>
        /// <returns>Enumerator of transforms.</returns>
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
