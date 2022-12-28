using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AzureHealth.DataServices.Security
{
    /// <summary>
    /// Interface to be implemented by TokenCache.
    /// </summary>
    public interface ITokenCache
    {
        /// <summary>
        /// Add Token value to cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task AddToken(string key, string value);

        /// <summary>
        /// Get the token from the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<string> GetToken(string key);

    }
}
