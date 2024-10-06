/// <summary>
/// Represents an exponential backoff mechanism for retrying failed operations.
/// </summary>
public class ExponentialBackoff
{
    private readonly int _maxRetryCount;
    private readonly double _backoffFactor;
    private readonly TimeSpan _maxBackoffDelay;
    private readonly TimeSpan _initialDelay;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExponentialBackoff"/> class.
    /// </summary>
    /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
    /// <param name="backoffFactor">The multiplier for the backoff delay.</param>
    /// <param name="maxBackoffDelay">The maximum delay between retry attempts.</param>
    /// <param name="initialDelay">The initial delay before the first retry.</param>
    public ExponentialBackoff(int maxRetryCount, double backoffFactor, TimeSpan maxBackoffDelay, TimeSpan initialDelay)
    {
        _maxRetryCount = maxRetryCount;
        _backoffFactor = backoffFactor;
        _maxBackoffDelay = maxBackoffDelay;
        _initialDelay = initialDelay;
    }

    /// <summary>
    /// Attempts to execute the provided operation asynchronously with exponential backoff retries.
    /// </summary>
    /// <param name="operation">A function that returns a task representing the operation to be retried.</param>
    /// <returns>
    /// A task that represents whether the operation succeeded (true) or failed after maximum retries (false).
    /// </returns>
    public async Task<bool> RetryAsync(Func<Task<bool>> operation)
    {
        TimeSpan currentDelay = _initialDelay;
        Random random = new Random(); // Random generator for jitter

        for (int attempt = 0; attempt < _maxRetryCount; attempt++)
        {
            if (await operation())
            {
                return true; // Success
            }

            // Apply exponential backoff with random jitter
            var jitter = random.Next(0, 100); // Add up to 100ms of jitter
            currentDelay = TimeSpan.FromMilliseconds(Math.Min(currentDelay.TotalMilliseconds * _backoffFactor + jitter, _maxBackoffDelay.TotalMilliseconds));

            Console.WriteLine($"[LOG] Retrying in {currentDelay.TotalMilliseconds}ms...");
            await Task.Delay(currentDelay);
        }

        return false; // Failure after retries
    }

}
