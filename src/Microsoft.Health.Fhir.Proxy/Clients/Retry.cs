using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Clients
{
    public static class Retry
    {
        public static void Execute(Action retryOperation)
        {
            Execute(retryOperation, TimeSpan.FromMilliseconds(250), 3);
        }

        public static void Execute(Action retryOperation, TimeSpan deltaBackoff, int maxRetries)
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
                    retryOperation();
                    return;
                }
                catch
                {
                    if (attempt == maxRetries)
                    {
                        throw;
                    }

                    Task.Delay(delayMilliseconds).GetAwaiter();
                    attempt++;
                }
            }

            throw new OperationCanceledException("Operation cancelled due to retry failure.");
        }

        public static async Task ExecuteAsync(Action retryOperation)
        {
            await ExecuteAsync(retryOperation, TimeSpan.FromMilliseconds(250), 3);
        }

        public static async Task ExecuteAsync(Action retryOperation, TimeSpan deltaBackoff, int maxRetries, ILogger logger = null)
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
                    retryOperation();
                    break;
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
        }
    }
}
