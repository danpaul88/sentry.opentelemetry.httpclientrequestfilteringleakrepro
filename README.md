# Introduction
This repository contains a simple ASP.Net service that reproduces a memory leak in the Sentry integration with OpenTelemetry when using a request filter with the OpenTelemetry HTTP client instrumentation.

# Running the service
Before running the service you must replace the dummy DSN `http://your.dsn.here` with a valid DSN

Once the DSN is configured simply start the service and using a memory profiler, note that `TransactionTracer` instances start accumulating inside `SentrySpanProcessor._map` which are never removed. These correlate to the number of filtered requests.

# Code Notes
The hosted service `OutgoingRequestsHostedService` sends requests to itself to trigger the behaviour.

The static class `HttpRequestFiltering` contains a simple method to decide whether to sample an `HttpRequestMessage` based on a random number. It also counts the number of collected vs filtered requests - note that the number of leaked instances correlates with the number of filtered requests, as reported to the console. Changing `shouldCollect` to always be true resolves the leak, demonstrating that the act of filtering a `HttpRequestMessage` appears to cause the leak.
