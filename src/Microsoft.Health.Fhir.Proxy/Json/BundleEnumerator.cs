using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Proxy.Json
{
    public class BundleEnumerator : IEnumerator<JToken>
    {
        public BundleEnumerator(JArray array, bool ifNoneExist)
        {
            this.array = array;
            this.ifNoneExist = ifNoneExist;
        }

        private JArray array;
        private int index = -1;
        private bool disposed;
        private readonly bool ifNoneExist;

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

        public void Reset()
        {
            index = -1;
        }

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
