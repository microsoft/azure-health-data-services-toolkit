using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureHealth.DataServices.Json
{
    /// <summary>
    /// Enumerator for FHIR bundle.
    /// </summary>
    public class BundleEnumerator : IEnumerator<JToken>
    {
        private readonly bool _ifNoneExist;
        private JArray _array;
        private int _index = -1;
        private bool _disposed;

        /// <summary>
        /// Creates an instance of BundleEnumerator.
        /// </summary>
        /// <param name="array">JArray containing items in the bundle.</param>
        /// <param name="ifNoneExist">FHIR ifNoneExists flag omits if false.</param>
        public BundleEnumerator(JArray array, bool ifNoneExist)
        {
            _array = array;
            _ifNoneExist = ifNoneExist;
        }

        /// <summary>
        /// Gets the current JToken for the enumerator.
        /// </summary>
        public JToken Current
        {
            get
            {
                try
                {
                    return _array[_index];
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
            _index++;

            if (_index == _array.Count)
            {
                return false;
            }

            if (_ifNoneExist)
            {
                while (_array[_index].IsNullOrEmpty() || _array.GetArrayItem<string>($"$[{_index}].request.ifNoneExist") == null)
                {
                    if (_index == _array.Count - 1)
                    {
                        return false;
                    }

                    _index++;
                }
            }
            else
            {
                while (_array[_index].IsNullOrEmpty())
                {
                    if (_index == _array.Count - 1)
                    {
                        return false;
                    }

                    _index++;
                }
            }

            return _index < _array.Count;
        }

        /// <summary>
        /// Resets the enumerator to the beginning.
        /// </summary>
        public void Reset()
        {
            _index = -1;
        }

        /// <summary>
        /// Disposes the enumerator.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="dispose">Indicator when true signals the object should be disposed.</param>
        protected void Dispose(bool dispose)
        {
            if (dispose & !_disposed)
            {
                _disposed = true;
                _array = null;
            }
        }
    }
}
