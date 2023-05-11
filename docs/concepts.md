# Concepts

The core goal of this toolkit is to build **custom operations** to extend the behavior of Azure Health Data Services. Abstractly, custom operations can:

- Modify incoming requests.
- Acquire additional information to make decisions.
- Output information to Azure services.

A custom operation is the business goal that you're trying to accomplish with this toolkit, and a pipeline is the implementation of the custom operation. 

## Definitions

- **Operation context**: Common object passed between components of a pipeline containing the request and response.
- **Pipeline**: Container for the actions of custom operations with filters, channels, and bindings executed in the order shown below.
  - **Filter:** A unit of action that modifies the request and/or result via the Operation Context. Filters can be chained together in a single input/output section of a pipeline.
  - **Channel:** Used to output data in a pipeline to an external system actor (ESA). This is usually an Azure service (like Storage, Event Hub, and/or Service Bus).
  - **Binding:** The target service for a custom operation (usually a FHIR service). This can be null for custom operations that don't need to have a destination.

### Custom operation/pipeline overview

![Pipeline overview](./images/pipeline.png)

## Pipelines

Pipelines allow you to hook into existing .NET hosting platforms to build custom operations. Currently, there are two types of pipelines that you can use in this toolkit.

- **WebPipeline** for use with ASP.NET Web APIs in Azure App Services or other ASP.NET hosting platforms.
- **Azure Function pipeline** for use with Azure Functions as an isolated process.

These pipelines allow you to hook into the configuration of ASP.NET and Azure Functions to use the other components below.

### Input/output section of pipeline

![Pipeline input output](./images/pipeline-input-output.png)

### Filters

Filters are a logical container for units of your custom operations. They are separated into input filters to modify the request before it goes to its destination (like FHIR Service). For complex custom operations, multiple filters can be chained together so pieces of logic can stay small, reusable, and testable.

Filters have a common interface which allows them to be used in a pipeline, since the properties, methods, and events can be hooked into the pipeline. Filters always input and output an operation context.

A Filter should be created from a factory and NOT be a singleton or static class. Rather, a filter instance should be generated per call.

To create a filter for custom logic, it must have:

| Name | Type | Description |
|------| ---- | ----------- |
| Id | Property | Unique Id for the filter (useful for logging). |
| Name | Property | Unique to the type of filter and can be used for creating the filter from a factory. |
| ExecuteAsync | Method | An async method with an OperationContext as an argument and returns an OperationContext. |
| OnFilterError | Event | An event that is a trigger for filter errors, which can be used to terminate and dispose the pipeline. |

### Channels

Channels are an abstract way to communicate information in a pipeline to and from outside services. In practice, you can use channels to send information to other Azure services like Blob Storage or a Service Bus. Channels are extensible - so custom channels can be built for any needed destination.

This toolkit has prebuilt channels from Azure Blob Storage, Azure Event Hubs, and Azure Service Bus. Channels can be send only, receive only, or send and receive. 

- Event Grid and Blob Storage channels are receive only.
- Event Hub and Service Bus can be receive only, send only, or send/receive.

All channels have:

| Name | Type | Description |
|------| ---- | ----------- |
| Id | Property | Unique Id for the filter (useful for logging). |
| Name | Property | Unique to the type of filter and can be used for creating the filter from a factory. |
| IsEncrypted | Property | Indicates whether the channel is encrypted. |
| IsConnected | Property | Indicates whether the channel is connected. |
| IsAuthenticated | Property | Indicates whether the channel has been authenticated. |
| Port | Property | The port used by the channel. |
| State | Property | Current channel state. A change SHOULD signal a StateChanged event. |
 OpenAsync | Method | Opens the channel. |
| CloseAsync | Method | Closes the channel. |
| SendAsync | Method | Sends a message on the channel. |
| ReceiveAsync | Method | Receives a message on the channel. |
| OnOpen | Event | Channel has opened. |
| OnClose | Event | Channel has closed. |
| OnError | Event | Channel has returned an error. |
| OnStateChange | Event | Chanel state has changed. |
| OnReceive | Event | Channel received a message. |

### Bindings

Bindings couple inputs and outputs in pipelines. The most common use of a binding is to send the current operation context to the FHIR service. This is done after zero or more input filters/channels and before zero or more output filter/channels.

| Name | Type | Description |
|------| ---- | ----------- |
| Id | Property | Unique Id of the binding. |
| Name | Property | Unique to the type of binding. |
| ExecuteAsync | Method | Executes the binding to send data. |
| BindingErrorEventArgs | Event | Signals an error on the binding. |
| BindingCompleteEventArgs | Event | Signals completion of a binding. |

## Authentication

Authentication is recommended via the [Azure Identity client library for .NET](https://learn.microsoft.com/dotnet/api/overview/azure/identity-readme?view=azure-dotnet). We recommend using `DefaultAzureCredential`. 

### Azure Authentication

When setting up a binding (like `RestBinding`), you just need to add a new instance of `DefaultAzureCredential` to the configuration. All calls to the binding target will now automatically be authenticated via this credential.

```csharp
services.AddBinding<RestBinding, RestBindingOptions>(options =>
{
    options.BaseAddress = "https://<workspace>-<fhir service>.fhir.azurehealthcareapis.com";
    options.Credential = new new DefaultAzureCredential();
})
```

### Authorization Header Forwarding

The RestBinding supports forwarding the `Authorization` header from the client to the binding target. This is useful for the simplest authentication configuration when routing requests between the FHIR Service and your custom operation via API Management. The client just needs to get an access token for the FHIR Service. The custom operation will simply use this token in the binding versus having to get it's own token.

```csharp
services.AddBinding<RestBinding, RestBindingOptions>(options =>
{
    options.BaseAddress = "https://<workspace>-<fhir service>.fhir.azurehealthcareapis.com";
    options.PassThroughAuthorizationHeader = true;
})
```