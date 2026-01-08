# Harkers News Test

## Premisses

-   There are 200 Ids in the list of the best stories
-   Environment with 8 vCPUs
-   Main Target: Protect the API from overload and high frequency of troghput
-   Addcional Targets:
    -   Low latency and near to real time
    -   The API will be REST, but it won`t folow the patterns needed to become REST FULL
-   Solution:
    -   Only one route with the path /v0/story/beststories.
    -   Clean Architecture to organize the api
    -   Refit to set the HttpClients
    -   A tree of execution to present the execution without the n parameter
    -   To paralelism add the SemaphoreSlim pattern, but I could use the ParallelOptions
    -   Concurrent requests are limited to vCPU * 2 to balance the processes
    -   InMemoryCache with 300 seconds to expiration was added to increase the perfomance
-   Solutions that could be added
    -    The API gateway should also has the max TPS configured in order to throttling the requests
    -   Apply Autoscale  in the cluster using CPU usage as metric
    -   Add Patterns like
        -   Notification to control business rules
        -   Polly to implement CircuitBreaker with retries and HeavyConcurrency policy to queue what exceeds the permited limit

### Requests Without parallelism

**Target:** Getting all best stories with no N parameter and without parallelism

E.g: GET [http://localhost:5012/v0/story/beststories](http://localhost:5012/v0/story/beststories "http://localhost:5012/v0/story/beststories")

Amout of requests using postman: 10 times

#### Latency:

- 1 request (without cache): between 32.4 seconds and 32.9 seconds
- 2 request (with cached data) : between 41 ms and 69 ms

### Requests With parallelism

**Target:** Getting all best stories with no N parameter and WITH parallelism

E.g: GET [http://localhost:5012/v0/story/beststories](http://localhost:5012/v0/story/beststories "http://localhost:5012/v0/story/beststories")

Amout of requests using postman: 10 times

#### Latency:

- 1 request (without cache): between 4 seconds and 6 seconds  
- 2 request (with cached data) : between 64 ms and 200 ms

#### Latency:

**Target:** Testing the increase to **the max of threads till 32 -  4 for each CPU**

**Amout of requests using postman: 10 times**

- 1 request (without cache): between 3.28 seconds and 4 seconds  
- 2 request (with cached data) : between 41 ms and 100 ms

**Target:** Getting best stories using the N parameter and WITH parallelism

E.g: GET [http://localhost:5012/v0/story/beststories?n=10](http://localhost:5012/v0/story/beststories?n=10 "http://localhost:5012/v0/story/beststories?n=10")

Amout of requests using postman: 10 times

#### Latency:

- 1 request (without cache): between 1.52 seconds and 1.70 seconds  
- 2 request (with cached data) : between 50 ms and 65ms

#### Latency:

**Target:** Getting best stories using the N parameter and increasing **the max of threads till 32 -  4 for each CPU**

**Amout of requests using postman: 10 times**

- 1 request (without cache): between 1.52 seconds and 1.70 seconds  
- 2 request (with cached data) : between 56 ms and 76 ms

