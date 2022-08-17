# Concepts

The core goal of this SDK is to build **custom operations** to extend the behavior of Azure Health Data Services. Abstractly, custom operations can:

- Modify incoming requests.
- Acquire additional information to make decisions.
- Output information to Azure services.

## Definitions

- **Operation Context**: Common object passed between components of a pipeline containing the request and response.
- **Pipeline**: Container for the actions of custom operations with filters, channels, and bindings executed in the order shown below.
- **Filter:** A unit of action that modifies the request and/or result via the Operation Context. Filters can be chained together in a single input/output section of a pipeline.
- **Channel:** Used to output data in a pipeline to an external system actor (ESA). This is usually an Azure service (like Storage, Event Hub, and/or Service Bus).
- **Binding:** The target service for a custom operation (usually a FHIR service). This can be null for custom operations that don't need to have a destination.

Custom Operation/Pipeline Overview |  Input/Output Section of Pipeline
:-------------------------:|:-------------------------:
![Pipeline overview](./images/pipeline.png)  | ![Pipeline input output](./images/pipeline-input-output.png)

## Pipelines

Pipelines allow you to hook into existing .NET hosting platforms to build custom operations. Currently, there are two types of pipelines that you can use.

- **WebPipeline** for use with ASP.NET Web APIs in Azure App Services or other ASP.NET hosting platforms.
- **Azure Function Pipeline** for use with Azure Functions as an isolated process.

These pipelines allow you to hook into the configuration of ASP.NET and Azure Functions to use the other components below.

### Filters

Filters are a logical container for units of your custom operations. They are separated into input filters to modify the request before it goes to its destination (like FHIR Service). For complex custom operations, multiple filters can be chained together so pieces of logic can stay small, reusable, and testable.

Filters have a common interface which allow them to be used in a pipeline, since the properties, methods, and events can be hooked into the pipeline. Filters always input and output an operation context.

A Filter should be created from a factory and NOT be a singleton or static class, rather an instance per call.

To create a filter for custom logic, they must have:

| Name | Type | Description |
|------| ---- | ----------- |
| Id | Property | Unique Id for the filter (useful for logging). |
| Name | Property | Unique to the type of filter that can be used for create the filter from a factory. |
| ExecuteAsync | Method | An async method with an OperationContext as a argument and returns an OperationContext. |
| OnFilterError | Event | An event that is a trigger for filter errors, which can be used to terminate and dispose the pipeline. |

### Channels

Channels are an abstract way to communicate information in a pipeline to outside services. In practice, you can use channels to send information to other Azure services like Blob Storage or Service Bus. Channels are extensible - so custom channels can be built for any needed destination.

Channels can be send only, receive only, send and receive. Examples: (1) A channel for an event hub that only sends to the event hub (2) A service bus channel that only receives from a specific topic (3) A TCP channel that can send and receive.

This SDK has prebuilt channels from Azure Blob Storage, Azure Event Hubs, and Azure Service Bus. All channels have:

| Name | Type | Description |
|------| ---- | ----------- |
| Id | Property | Unique Id for the filter (useful for logging). |
| Name | Property | Unique to the type of filter that can be used for create the filter from a factory. |
| IsEncrypted | Property | Indicates whether the channel is encrypted. |
| IsConnected | Property | Indicates whether the channel is connected. |
| IsAuthenticated | Property | Indicates whether the channel has authenticated. |
| Port | Property | The port used by the channel. |
| State | Property | Current channel state. A change SHOULD signal a StateChanged event. |
 OpenAsync | Method | Opens the channel. |
| CloseAsync | Method | Closes the channel. |
| SendAsync | Method | Sends a message on the channel. |
| ReceiveAsync | Method | Receives a message on the channel. |
| OnOpen | Event | Channel has opened. |
| OnClose | Event | Channel has closed. |
| OnError | Event | Channel has errored. |
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

## Authenticator

Authenticator is a flexible class to help acquire an access token for calling the FHIR Service and other Azure Services. You have two options for configuring the authenticator for use in your custom operations:

1. Without any explicit configuration or reference in your code to the authentication method or settings. This leverages [DefaultAzureCredential](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme#defaultazurecredential).
2. With explicit selection of the authorization method and configuration via the settings on the authenticator class.

You get to choose which method is best for your custom operation. We recommend starting with the non-explicit method that uses `DefaultAzureCredential` as it is intended to simplify getting started with custom operations. The injection of authentication method via the environment works great for secret-less connection both in local development and cloud deployment. If you want more control over your authentication, you can explicitly define this when you call the authenticator class.

| Authentication Method | Configuration Method | Description
|------| ---- | ----------- |
| Managed Identity | Explicit or implicit  | Attempts authentication using a managed identity that has been assigned to the deployment environment |
| Client Credentials | Explicit or implicit | Uses a service principal to connect. Either via a secret or certificate. |
| On-Behalf-Of | Explicit only | Calls the endpoint on-behalf-of the caller, propagating the original caller'r identify and permissions. |
| Visual Studio | Implicit only | Uses the Azure session from Visual Studio. |
| Visual Studio Code | Implicit only | Uses the Azure session from Visual Studio Code. |
| Azure CLI | Implicit only | Uses the Azure session from the Azure CLI. |
| Azure PowerShell | Implicit only | Uses the Azure session from the Azure PowerShell. |

## Implicit Configuration

To use the authenticator implicitly leveraging `DefaultAzureCredential`, add the authenticator to your custom operation *without* any parameters. The authenticator now will either automatically pull the needed information from your system (mainly for development or managed identity) or you can configure via configuring the environment [like DefaultAzureCredential](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme#defaultazurecredential).

```csharp
services.UseAuthenticator();
```

### Explicit Configuration

When explicitly defining authentication configuration, you must define the configuration when adding the authenticator to your custom operation. For example, this code explicitly sets the authentication type to `ClientSecret` and the `ClientId`, `ClientSecret`, and `TenantId` from the application configuration (often passed in from environment variables or Azure KeyVault).

```csharp
services.UseAuthenticator(options =>
  {
      options.CredentialType = ClientCredentialType.ClientSecret;
      options.ClientId = config.ClientId;
      options.ClientSecret = config.ClientSecret;
      options.TenantId = config.TenantId;
  });
```

## Clients

This SDK has a built-in REST client (called `RestRequest`) which abstracts the logic for a resilient client needed for REST requests for cloud services. This client is used for bindings in a pipeline, but this client is also useful for calling the FHIR Service (or external REST services) inside of filters to gather additional information needed for a custom operation.

The `RestRequestBuilder` class is also available for an easy, builder style creation of a `RestRequest` client. See [RestBinding.cs](/src/DataServices/Bindings/RestBinding.cs) for an example of how this is used.
