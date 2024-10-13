## for testing:
* (for local uncomment program.cs local part)
* docker network create redis-network
* docker-compose build
* docker compose up
## Description of the Problem

The project comprises two layers: Service and Backend.


# Exponential Backoff Parameters

## `_maxRetryCount` (Maximum Number of Retries)
- **What it does**: Determines how many times the system will retry the operation before giving up.
- **Best Value**:
  - For low-contention environments: 3-5 retries are typical.
  - For high-contention environments where resource locks are common: 5-8 retries might be more appropriate.
- **Recommendation**: Start with 5 retries.

## `_backoffFactor` (Backoff Multiplier)
- **What it does**: The factor by which the delay increases after each failed attempt. 
  - If the factor is 2.0, the wait time doubles after each retry (e.g., 100ms -> 200ms -> 400ms).
- **Best Value**:
  - Lower factor (e.g., 1.5-2.0): Good for minimizing the wait time while still exponentially increasing delays.
  - Higher factor (e.g., 3.0 or more): More aggressive backoff, useful if contention is frequent and you want to avoid too many retries in a short time.
## `_maxBackoffDelay` (Maximum Delay Between Retries)
- **What it does**: Caps the maximum delay between retry attempts. Even if the backoff factor suggests a larger delay, this value limits it.
- **Best Value**:
  - Low max backoff (e.g., 1-2 seconds): For operations that need to complete quickly (e.g., user-facing systems).
  - High max backoff (e.g., 10-30 seconds): For operations where longer delays are acceptable (e.g., background jobs or high-contention environments).

## `_initialDelay` (Initial Delay Before First Retry)
- **What it does**: The time the system waits before the first retry.
- **Best Value**:
  - Lower initial delay (e.g., 50-100ms): If you expect that locks might be released quickly.
  - Higher initial delay (e.g., 200-500ms): If the resource is often locked for longer durations.

