# Harkers News Test

  

Premissas

In production, the service will run in a container with 4 vCPUs and 512 MB RAM.

  

Target: Protect the API from overload

Solution: SemaphoreSlim

Since the workload is I/O-bound (External HHtp Call), concurrent requests are limited to 8 to balance throughput and resource usage while protecting the external API from overload. Although the api gateway can also has a TPS limit in order to avoid unexpected requests.

  

Using the development machine

8 CPUs

Using semaphore pattern on repository with max of 16 threads, 2 for each CPU.

Semaphore used  to reduce the impact of fan out.

Latency:

1 request (without cache): between 4 seconds and 6 seconds  
2 request (with cached data) : between 64 ms and 200 ms