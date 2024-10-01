public class ExponentialBackoff
{
    private readonly int _maxRetryCount;
    private readonly double _backoffFactor;
    private readonly TimeSpan _maxBackoffDelay;
    private readonly TimeSpan _initialDelay;

    public ExponentialBackoff(int maxRetryCount, double backoffFactor, TimeSpan maxBackoffDelay, TimeSpan initialDelay)
    {
        _maxRetryCount = maxRetryCount;
        _backoffFactor = backoffFactor;
        _maxBackoffDelay = maxBackoffDelay;
        _initialDelay = initialDelay;
    }

    public async Task<bool> RetryAsync(Func<Task<bool>> operation)
    {
        TimeSpan currentDelay = _initialDelay;
        for (int attempt = 0; attempt < _maxRetryCount; attempt++)
        {
            if (await operation())
            {
                return true; // Success
            }

            currentDelay = TimeSpan.FromMilliseconds(Math.Min(currentDelay.TotalMilliseconds * _backoffFactor, _maxBackoffDelay.TotalMilliseconds));
            Console.WriteLine($"[LOG] Retrying in {currentDelay.TotalMilliseconds}ms...");
            await Task.Delay(currentDelay);
        }

        return false; // Failure after retries
    }
}
