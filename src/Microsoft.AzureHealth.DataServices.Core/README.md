## Microsoft.AzureHealth.DataServices.Core

### Overview
.NET 6 SDK for creating custom operations for FHIR in Azure Health Data Services.
- [Getting Started](https://github.com/microsoft/azure-health-data-services-toolkit#getting-started)
- [Conceptual Docs](https://github.com/microsoft/azure-health-data-services-toolkit/blob/main/docs/concepts.md)
- For more information, please see the Github repository [here.](https://github.com/microsoft/azure-health-data-services-toolkit)

### Features
- *Pipelines* to modify FHIR requests or responses.
  - *Filters* modify input and/or output
  - *Channels* send input and/or output to other services
  - *Bindings* provide a mechanism to link input and output pipelines
- *Json Transforms* enable FHIR metadata to be modified through configuration
- Supports REST enabled endpoint on a variety of hosts
  - Azure Functions
  - Web Sites
  - Azure Kubernetes Service (AKS)
  - Other .NET hosts
