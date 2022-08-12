# Azure Health Data Services SDK Samples

**TODO** - *add line about the problem/goal of the samples - showing what the SDK can be used for*

## Sample Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download))
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Azure Developer CLI](https://docs.microsoft.com/azure/developer/azure-developer-cli/get-started?tabs=bare-metal%2Cwindows&pivots=programming-language-csharp#prerequisites)
  - Quick install command on Windows: `powershell -ex AllSigned -c "Invoke-RestMethod 'https://aka.ms/install-azd.ps1' | Invoke-Expression"`
  - Quick install command on Linux/MacOS: `curl -fsSL https://aka.ms/install-azd.sh | bash`
- Visual Studio or Visual Studio Code
  - Or another code editor and proficiency with the command line
- Azure subscription with permissions to create resource groups, resources, and create role assignments.

## Quickstart

Use [this quickstart sample](./Quickstart/README.md) to see some common ways the SDK can be used to modify requests to the FHIR service. Is this quickstart we'll cover:

- Getting an access token for the FHIR service.
- Adding a header to audit the original requestor.
- Using a transform for a basic modification of FHIR server behavior.
- Using a pipeline with input and output filters to modify the request and response.

## Feature Samples

These samples go deep into individual features areas of this SDK, showing you how the individual pieces work so you can use them to build custom operations that fit your needs.

| Sample | Description |
| --- | --- |
| [Authenticator](./Authenticator/README.md) | Shows how you can get a token for accessing Azure resources inside your custom operation. |

## Use Case Samples

These samples will show you how to implement a real FHIR customization use case, end-to-end.

*Coming soon!*
