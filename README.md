# Azure Health Data Services SDK

The Azure Health Data Services SDK helps you extend the functionality of Azure Health Data Services by providing a consistent toolset to help you easily customize or modify the core service behavior. With the growth of healthcare data on the cloud, we’ve found that developers need to have custom behavior on top of our managed services. We’ve abstracted common patterns for integrating Azure Health Data Services so you can focus on your application to deliver more.

## FHIR Use Cases

Some example scenarios:

- FHIR operations not [supported by the FHIR Service](https://docs.microsoft.com/azure/healthcare-apis/fhir/fhir-features-supported#extended-operations) yet.
- Transforming request and/or response data.
- #TODO - add more

### What about FHIR Proxy? 

- This is an SDK vs a ready to deploy solution 
- FHIR Proxy mediated all requests, this SDK is meant to be for specific endpoints 
- This is more flexible  
- Higher code quality and testing 

## NuGet Packages

| Package Name | Description | |
| --- | --- | --- |
| [Azure.Health.DataServices.Core](https://www.nuget.org/packages/Microsoft.Health.DataServices.Core/) | .NET 6 SDK for creating custom operations to when using Azure Health Data Services. |
| [Azure.Health.DataServices.Channels.Extensions](https://www.nuget.org/packages/Microsoft.Health.DataServices.Channels.Extensions/) | .NET 6 SDK for extending channels using Azure Health Data Services. |
| [Azure.Health.DataServices.Caching](https://www.nuget.org/packages/Microsoft.Health.DataServices.Caching/) | .NET 6 SDK to simplify Azure storage operations when using Azure Health Data Services. |
| [Azure.Health.DataServices.Storage](https://www.nuget.org/packages/Microsoft.Health.DataServices.Storage/) | .NET 6 SDK for adding caching using Azure Health Data Services. |

## Getting Started

<Link to quickstart or some basic guide for those who aren’t patient>. 

This SDK can be integrated into new and existing .NET 6.0 applications. It is compute-agnostic, for example, on Azure Functions, Azure App Service, AKS, or virtual machines. You can interact with this SDK by installing the packages from NuGet and integrating them into your solution.  

## Samples

Check out our samples of [how to use the SDK here](./samples/README.md).

## Features

*Add some high-level feature list or definitions here*.

## Resources

- [FHIR Service Documentation](https://docs.microsoft.com/azure/healthcare-apis/fhir/overview)
- [FHIR Server OSS Repository](https://github.com/microsoft/fhir-server)

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
