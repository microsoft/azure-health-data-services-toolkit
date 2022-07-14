using System;
using System.Threading.Tasks;
using DataServices.Pipelines;

namespace DataServices.Bindings
{
    /// <summary>
    /// Interface to be implemented by a Binding.
    /// </summary>
    public interface IBinding
    {
        /// <summary>
        /// Gets the name of the binding.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the unique id on the binding instance.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// An event that signals an error in the binding.
        /// </summary>
        event EventHandler<BindingErrorEventArgs> OnError;

        /// <summary>
        /// An event that signals an error in the binding.
        /// </summary>
        event EventHandler<BindingCompleteEventArgs> OnComplete;

        /// <summary>
        /// Executes binding.
        /// </summary>
        /// <param name="context">Operation context.</param>
        /// <returns>Operation context.</returns>
        Task<OperationContext> ExecuteAsync(OperationContext context);
    }
}
