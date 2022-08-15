## Azure.Health.DataServices.Core

### Overview
.NET 6 SDK for creating custom operations for FHIR in Azure Health Data Services.
- [Getting Started](https://github.com/Azure/health-data-services-sdk#getting-started)
- [Conceptual Docs](https://github.com/Azure/health-data-services-sdk/blob/main/docs/concepts.md)

### Features
- *Pipelines* to modify FHIR requests or responses.
  - *Filters* modify input and/or output
  - *Channels* send input and/or to other services
  - *Bindings provide* mechanism to link input and output pipelines
- *Json Transforms* enabled FHIR metadata to be modified through configuration
- Supports REST enabled endpoint on a variety of hosts
  - Azure Functions
  - Web Sites
  - Azure Kubernetes Service (AKS)
  - Other .NET hosts