using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Clients
{
    public static class Retry
    {
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

                    Task.Delay(delayMilliseconds).GetAwaiter().GetResult();
                    attempt++;
                }
            }

            throw new OperationCanceledException("Operation cancelled due to retry failure.");
        }
    }
}
