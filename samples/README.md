# Azure Health Data Services Toolkit Samples

These samples show how this toolkit works for a few sample scenarios:

- **Quickstart**: A simple example showing how to setup, test, and deploy a custom operation.
- **Feature Samples**: Small projects showing how the toolkit features work. Local only.

## Sample prerequisites

- An Azure account with an active subscription.
  - You need access to create resource groups, resources, and role assignments in Azure
- [.NET 6.0](https://dotnet.microsoft.com/en-us/download)
- Visual Studio or Visual Studio Code
  - Or another code editor and proficiency with the command line

For samples that work with resources or deploy code to Azure (like the Quickstart), you will need:

- [Azure Command-Line Interface (CLI)](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Azure Developer CLI](https://docs.microsoft.com/azure/developer/azure-developer-cli/get-started?tabs=bare-metal%2Cwindows&pivots=programming-language-csharp#prerequisites)
- If you are using Visual Studio:
  - Azure Functions Tools. To add Azure Function Tools, include the Azure development workload in your Visual Studio installation.
- If you are using Visual Studio Code:
  - [Azure Function Core Tools 4](https://docs.microsoft.com/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash#install-the-azure-functions-core-tools)


## Quickstart

Use [quickstart sample](./Quickstart/) to see a basic end-to-end deployment of the Azure Health Data Services toolkit.

- Getting an access token for the FHIR service.
- Adding a header to audit the original requestor.
- Using a transform for a basic modification of FHIR server behavior.
- Using a pipeline with input and output filters to modify the request and response.

## Feature samples

These samples go deep into individual feature areas of this toolkit, showing you how the individual pieces work so you can use them to build custom operations that fit your needs. These samples are not meant to be deployed to Azure, but executed on your local machine.


| Sample | Description |
| --- | --- |
| [Authenticator](./FeatureSamples/Authenticator/) | Shows how you can get a token for accessing Azure resources inside your custom operation. |
| [Blob Channel](./FeatureSamples/BlobChannel/) | Shows how to use a Blob Channel. |
| [Custom Headers](./FeatureSamples/CustomHeaders/) | Shows how to inject custom headers. |
| [Event Grid Channel](./FeatureSamples/EventGridChannel/) | Shows how to use an Event Grid Channel. |
| [Event Hub Channel](./FeatureSamples/EventHubChannel/) | Shows how to use an Event Hub Channel. |
| [JSON Transform](./FeatureSamples/JsonTransform/) | Shows how to easily transform a JSON payload. |
| [Memory/Blob Cache](./FeatureSamples/MemoryCacheAndBlobProvider/) | Shows how to use a memory cache with Azure Blob storage backing. |
| [Memory/Redis Cache](./FeatureSamples/MemoryCacheAndRedisProvider/) | Shows how to use a memory cache with Redis backing. |
| [Service Bus Channel](./FeatureSamples/ServiceBusChannel/) | Shows how to use a Service Bus Channel. |
| [Simple External API](./FeatureSamples/SimpleExternalApiCustomOperation/) | Shows how an external API could be used with a custom operation. |

## Use Case Samples

These samples will show you how to implement a real FHIR customization use case, end-to-end.

*Coming soon!*
