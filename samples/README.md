# Azure Health Data Services SDK Samples

These samples show how this SDK works for a few sample scenarios, and includes a Quickstart as well as feature samples:

- **Quickstart**: A simple example showing how to setup, test, and deploy a custom operation.
- **Feature Samples**: Small projects showing how a SDK feature work. Local only.

## Sample Prerequisites

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

Use [this quickstart sample](./Quickstart/) to see some common ways the SDK can be used to modify requests to the FHIR service. Is this quickstart we'll cover:

- Getting an access token for the FHIR service.
- Adding a header to audit the original requestor.
- Using a transform for a basic modification of FHIR server behavior.
- Using a pipeline with input and output filters to modify the request and response.

## Feature Samples

These samples go deep into individual features areas of this SDK, showing you how the individual pieces work so you can use them to build custom operations that fit your needs.These samples are not meant to be deployed to Azure, but executed on your local machine.

| Sample | Description |
| --- | --- |
| [Authenticator](./Authenticator/) | Shows how you can get a token for accessing Azure resources inside your custom operation. |

## Use Case Samples

These samples will show you how to implement a real FHIR customization use case, end-to-end.

*Coming soon!*
