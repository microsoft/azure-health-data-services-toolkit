# Azure Health Data Services Toolkit Tests

These tests rely on a series of Azure resources to properly execute. These include:

- KeyVault
- Azure Storage
- Azure Event Hub
- Azure Event Grid
- Service Bus
- Redis

To setup your system for testing, add these values as .NET user secrets like below:

```json
{
  "ClientId": "<client-id>",
  "ClientSecret": "<client-secret>",
  "TenantId": "<tenant-id>",
  "FhirServerUrl": "foobar", // can stay foobar

  "StorageConnectionString": "<general-storage-connection-string>",

  "KeyVaultUri": "<general-keyvault-uri>",
  "KeyVaultCertificateName": "localhost",

  "LogLevel": "Information",
  "InstrumentationKey": "<general-app-insights-key>",

  "BlobStorageChannelConnectionString": "<general-storage-connection-string>",
  "BlobStorageChannelContainer": "blob-tests",

  "CacheConnectionString": "<general-redis-connection-string>",

  "EventGridBlobConnectionString": "<general-storage-connection-string>",
  "EventGridBlobContainer": "eventgrid-tests",
  "EventGridDataVersion": "1.0",
  "EventGridEventType": "Proxy",
  "EventGrid_Message_Queue": "eventgridqueue",
  "EventGrid_Reference_Queue": "eventgrid-ref",
  "EventGridSubject": "Test",
  "EventGridTopicAccessKey": "<event hub access Key>",
  "EventGridTopicUriString": "<event hub topic uri>",
  "EventHubConnectionString": "<general-eventhub-string>",
  "EventHubName": "test",
  "EventHubProcessorContainerName": "hub",
  "EventHubSku": "Basic",
  "EventHubBlobConnectionString": "<general-storage-connection-string>",
  "EventHubBlobContainer": "eventhub-blobs",

  "ServiceBusConnectionString": "<general-service-bus-connection-string>",
  "ServiceBusQueue": "testqueue",
  "ServiceBusSku": "Standard",
  "ServiceBusSubscription": "sub1",
  "ServiceBusTopic": "test",
  "ServiceBusBlobContainerName": "servicebus-blobs",
  "ServiceBusBlobConnectionString": "<general-storage-connection-string>"
}
```