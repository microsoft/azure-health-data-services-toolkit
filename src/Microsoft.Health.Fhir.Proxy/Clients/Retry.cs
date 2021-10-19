using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Clients
{
    public static class Retry
    {
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
                        logger?.LogWarning($"Retry max attempts {maxRetries} exceeded.");
                        logger?.LogError(ex, "Retry max attempts exceeded.");
                        throw;
                    }
                    else
                    {
                        logger?.LogWarning($"Retry attempt {attempt + 1} with delay {delayMilliseconds}ms.");
                        logger?.LogError(ex, "Retrying due to exception.");
                    }

                    await Task.Delay(delayMilliseconds);
                    attempt++;
                }
            }

            throw new OperationCanceledException("Operation cancelled due to retry failure.");
        }
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
                        logger?.LogWarning($"Retry max attempts {maxRetries} exceeded.");
                        logger?.LogError(ex, "Retry max attempts exceeded.");
                        throw;
                    }
                    else
                    {
                        logger?.LogWarning($"Retry attempt {attempt + 1} with delay {delayMilliseconds}ms.");
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
