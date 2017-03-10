# Service Fabric Logging

Logging, while straight forward (to me anyway), can often require composing various bits together to produce the "right setup"; in this case Service Fabric and Stateless/Stateful Service logging. Today
there seems to be a lot of "adapting" that must be done to hook into the logging APIs exposed in the different platforms. 

This example aims to demonstrate one way hooking things up. My intention is to jog your mind toward laying out a full fledged implementation of your own.

## Components Overview
* ServiceFabric (Service Fabric Service)
* ASP.NET Core (Weblistener) targeting .NET Framework 4.6.1
* [Serilog](https://serilog.net/)
* [EventFlow](https://github.com/Azure/diagnostics-eventflow) (The Microsoft Diagnostic one; not to be confused with the component for event sourcing)
* Elasticsearch + Kibana

# Overview

The "root" for the logging is Serilog. I chose Serilog because of the following:

* Decouple logging from ASP.NET Core 
* Flexible enrichment capabilities
* Configurable through code and XML/JSON configuration

## Why Elasticsearch?

Elasticsearch gave the most open ended indexing capabilities, and  didn't "artificially" limit the amount of events pushed to the data store. Need more capacity? Then increase the ES deployment capacity.

### deployment

Deployment of Elasticsearch and Kibana is on a single VM using Docker. I borrowed the Resource Manager template directly from [azure-quickstart-templates](https://github.com/Azure/azure-quickstart-templates/tree/master/docker-kibana-elasticsearch)

## Important Packages (NuGet)

### Serilog
* Serilog
* Serilog.Extensions.Logging
* Serilog.Settings.Configuration
* Serilog.Sinks.Literate

### EventFlow

* Microsoft.Diagnostics.EventFlow
* Microsoft.Diagnostics.EventFlow.ServiceFabric
* Microsoft.Diagnostics.EventFlow.Inputs.Etw
* Microsoft.Diagnostics.EventFlow.Inputs.EventSource
* Microsoft.Diagnostics.EventFlow.Inputs.Serilog
* Microsoft.Diagnostics.EventFlow.Outputs.ElasticSearch
* Microsoft.Diagnostics.EventFlow.Outputs.StdOutput

# Guidance / Commentary

## Serilog
When using Serilog for a SF Service, do NOT use the "global" Log.Logger. As noted [here](https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-diagnostics-overview):

> Service Fabric can host multiple instances of the same service type within a single process. 
> If you use the static Log.Logger, the last writer of the property enrichers will show values for all instances that are running. 
> This is one reason why the _logger variable is a private member variable of the service class. 
> Also, you must make the _logger available to common code, which might be used across services.

It's not complex to figure out how NOT to do this. However, the code in _LoggingConfigurator_ and _Program_ show how I did this.

## ASP.NET Core
Logging's cross-cutting concern should guide you *away from coupling your code to anything ASP.NET Core*. 

### Logging middleware
But given we're trying to capture all HTTP requests going through it, Serilog is adapted into it, and logging Middleware was created. The Middleware does the following:

* Logs all HTTP requests (any verb) including the request/response payload
* Logs exceptions occuring in _any_ controller
* Centralizes the hook into handling exceptions and setting the appropriate status code: 403, 404, 500, etc.

> Credit to [Nicholas Blumhardt's](https://nblumhardt.com/) [post for Seq](http://blog.getseq.net/smart-logging-middleware-for-asp-net-core/) on the Middleware groundwork it laid for me.