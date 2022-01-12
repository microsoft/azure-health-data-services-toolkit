## Microsoft.Health.Fhir.Proxy

### Overview
.NET 6 SDK for creating custom operations to when using the Microsoft API for FHIR.
- [API Definitions](https://github.com/microsoft/fhir-proxy-sdk/blob/main/docs/reference/toc.html)

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
	- etc.