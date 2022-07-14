using System;
using System.Threading.Tasks;
using DataServices.Pipelines;

namespace DataServices.Filters
{
    /// <summary>
    /// IFilter interface to be implemented by filters.
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Event signals the filter caught an error.
        /// </summary>
        event EventHandler<FilterErrorEventArgs> OnFilterError;

        /// <summary>
        /// Gets the unique id on the filter instance.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the name of the filter.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the requirement for executing the filter.
        /// </summary>
        StatusType ExecutionStatusType { get; }

        /// <summary>
        /// Executes the filter operation.
        /// </summary>
        /// <param name="context">Context of the input for filter execution.</param>
        /// <returns>Context for input to next filter or output for http response.</returns>
        Task<OperationContext> ExecuteAsync(OperationContext context);
    }
}
