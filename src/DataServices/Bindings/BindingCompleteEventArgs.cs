using System;
using DataServices.Pipelines;

namespace DataServices.Bindings
{
    /// <summary>
    /// Event args for binding complete.
    /// </summary>
    public class BindingCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Creates an instance of BindingCompleteEventArgs.
        /// </summary>
        /// <param name="id">Instance ID of the binding.</param>
        /// <param name="name">Name of binding.</param>
        /// <param name="context">OperationContext</param>
        public BindingCompleteEventArgs(string id, string name, OperationContext context)
        {
            Id = id;
            Name = name;
            Context = context;
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
        /// Gets the OperationContext.
        /// </summary>
        public OperationContext Context { get; private set; }
    }
}
