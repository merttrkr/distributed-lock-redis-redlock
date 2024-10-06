# AdCreative.ai Integration Case
## for testing:
* (for local uncomment program.cs local part)
* docker network create redis-network
* docker-compose build
* docker compose up
## Description of the Problem

The project comprises two layers: Service and Backend.

For brevity, the API or presentation layer is excluded from the description.

This represents a typical integration scenario where items are externally sent by a third party to our integration service. The service is called with only the item content, to which we assign internal incremental identifiers, returning them (in text form here) to the third party.

The rule dictates that any item content should be saved only once and not occur twice in our systems. As per the agreement, the third party should wait for the result of their call, which can take a while (simulated as two seconds here, but realistically closer to forty seconds). However, in reality, the third party calls the service multiple times without waiting for a response.

Although protection is in place to check for duplicate items, if called rapidly in parallel (as demonstrated in the main Program), multiple entries with the same content are recorded on our end.

## Required Solution

### 1- Single Server Scenario

**a: Solution Description:**
- Modify the code exclusively within the Service layer (folder) to ensure that the same content is never saved more than once under any circumstances.
- Ensure that items with different content are processed concurrently without waiting for each other.

**b: Implementation:**
- Implement the solution within the Service layer.

**c: Demonstration in Program.cs:**
- Add code to Program.cs to showcase that the implemented solution allows parallel storage of items with different content.

### 2 - Distributed System Scenario

**a: Solution Description:**
- In case of multiple servers containing ItemIntegrationService, implement a solution for the distributed system scenario.

**b: Weaknesses:**
- Identify and describe any weaknesses that the solution might have in a text file.

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

