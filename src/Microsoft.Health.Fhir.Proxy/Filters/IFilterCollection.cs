using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Proxy.Filters
{
    public interface IFilterCollection : IList<IFilter>
    {
    }
}
