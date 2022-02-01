using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Fhir.Proxy.Json
{
    /// <summary>
    /// Enumerator for FHIR bundle.
    /// </summary>
    public class BundleEnumerator : IEnumerator<JToken>
    {
        /// <summary>
        /// Creates an instance of BundleEnumerator.
        /// </summary>
        /// <param name="array">JArray containing items in the bundle.</param>
        /// <param name="ifNoneExist">FHIR ifNoneExists flag omits if false.</param>
        public BundleEnumerator(JArray array, bool ifNoneExist)
        {
            this.array = array;
            this.ifNoneExist = ifNoneExist;
        }

        private JArray array;
        private int index = -1;
        private bool disposed;
        private readonly bool ifNoneExist;

        /// <summary>
        /// Gets the current JToken for the enumerator.
        /// </summary>
        public JToken Current
        {
            get
            {
                try
                {
                    return array[index];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        object IEnumerator.Current => Current;

        /// <summary>
        /// Moves the enumerator forward by one.
        /// </summary>
        /// <returns>True if item is available; otherwise false.</returns>
        public bool MoveNext()
        {
            index++;

            if (index == array.Count)
            {
                return false;
            }

            if (ifNoneExist)
            {
                while ((array[index].IsNullOrEmpty() || array.GetArrayItem<string>($"$[{index}].request.ifNoneExist") == null))
                {
                    if (index == array.Count - 1)
                    {
                        return false;
                    }

                    index++;
                }
            }
            else
            {
                while (array[index].IsNullOrEmpty())
                {
                    if (index == array.Count - 1)
                    {
                        return false;
                    }

                    index++;
                }
            }

            return (index < array.Count);
        }

        /// <summary>
        /// Resets the enumerator to the beginning.
        /// </summary>
        public void Reset()
        {
            index = -1;
        }

        /// <summary>
        /// Disposes the enumerator.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }

        protected void Dispose(bool dispose)
        {
            if (dispose & !disposed)
            {
                disposed = true;
                array = null;
            }
        }

    }
}
