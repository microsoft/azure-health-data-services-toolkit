using System.Collections.Generic;

namespace Azure.Health.DataServices.Filters
{
    /// <summary>
    /// An interface for a collection of filters.
    /// </summary>
    public interface IFilterCollection : IList<IFilter>
    {
    }
}
