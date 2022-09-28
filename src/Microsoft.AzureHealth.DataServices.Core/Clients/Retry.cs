using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureHealth.DataServices.Clients
{
    /// <summary>
    /// Async http request retry logic.
    /// </summary>
    public static class Retry
    {
        /// <summary>
        /// Executes an http request with retry logic.
        /// </summary>
        /// <param name="request">Rest request to send.</param>
        /// <param name="deltaBackoff">Time to wait for retry if request fails.</param>
        /// <param name="maxRetries">Maxiumum number of times to retry failed requests.</param>
        /// <param name="logger">ILogger</param>
        /// <returns>HttpResponseMessage</returns>
        public static async Task<HttpResponseMessage> ExecuteRequest(RestRequest request, TimeSpan deltaBackoff, int maxRetries, ILogger logger = null)
        {
            int delayMilliseconds = Convert.ToInt32(deltaBackoff.TotalMilliseconds);
            if (maxRetries < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetries));
            }

            int attempt = 0;
            while (attempt < maxRetries)
            {
                try
                {
                    var response = await request.SendAsync();
                    if (response.IsSuccessStatusCode || response.StatusCode != HttpStatusCode.Conflict)
                    {
                        return response;
                    }
                    else
                    {
                        throw new RetryException("Status code indicates retry required.");
                    }
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                    {
                        logger?.LogWarning("Retry max attempts {MaxRetries} exceeded.", maxRetries);
                        logger?.LogError(ex, "Retry max attempts exceeded.");
                        throw;
                    }
                    else
                    {
                        logger?.LogWarning("Retry attempt {Attempt} with delay {delay}ms.", attempt + 1, delayMilliseconds);
                        logger?.LogError(ex, "Retrying due to exception.");
                    }

                    await Task.Delay(delayMilliseconds);
                    attempt++;
                }
            }

            throw new OperationCanceledException("Operation cancelled due to retry failure.");
        }

        /// <summary>
        /// Executes an http request with retry logic.
        /// </summary>
        /// <typeparam name="T">The type return by the executing function.</typeparam>
        /// <param name="func">Function that executes the rest request.</param>
        /// <param name="deltaBackoff">Time to wait for retry if request fails.</param>
        /// <param name="maxRetries">Maximum number of times to retry failed requests.</param>
        /// <param name="logger">ILogger</param>
        /// <returns>Type returned by the executing function.</returns>
        public static async Task<T> Execute<T>(Func<Task<T>> func, TimeSpan deltaBackoff, int maxRetries, ILogger logger = null)
        {
            int delayMilliseconds = Convert.ToInt32(deltaBackoff.TotalMilliseconds);
            if (maxRetries < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetries));
            }

            int attempt = 0;
            while (attempt < maxRetries)
            {
                try
                {
                    return await func();
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                    {
                        logger?.LogWarning("Retry max attempts {MaxRetries} exceeded.", maxRetries);
                        logger?.LogError(ex, "Retry max attempts exceeded.");
                        throw;
                    }
                    else
                    {
                        logger?.LogWarning("Retry attempt {Attempt} with delay {Delay}ms.", attempt + 1, delayMilliseconds);
                        logger?.LogError(ex, "Retrying due to exception.");
                    }

                    await Task.Delay(delayMilliseconds);
                    attempt++;
                }
            }

            throw new OperationCanceledException("Operation cancelled due to retry failure.");
        }
    }
}
