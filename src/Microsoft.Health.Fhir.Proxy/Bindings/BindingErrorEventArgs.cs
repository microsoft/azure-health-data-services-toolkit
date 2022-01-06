using System;

namespace Microsoft.Health.Fhir.Proxy.Bindings
{
    /// <summary>
    /// Event args for binding error events.
    /// </summary>
    public class BindingErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Creates an instance of BindingErrorEventArgs.
        /// </summary>
        /// <param name="id">Instance ID of the binding.</param>
        /// <param name="name">Name of the binding.</param>
        /// <param name="error">Exception thrown in the binding.</param>
        public BindingErrorEventArgs(string id, string name, Exception error)
        {
            Id = id;
            Name = name;
            Error = error;
        }
        /// <summary>
        /// Gets the name of the binding.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the instance ID of the binding.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Exception throw in the binding.
        /// </summary>
        public Exception Error { get; private set; }
    }
}
