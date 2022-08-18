# Channel to Azure Event Hubs

In this sample, a pipeline is created with a channel to send data to Azure Event Hubs.

## Concepts

This sample shows how to configure a channel to send pipeline data to Azure Event Hubs.


## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.
- An authenticated Azure environment.
  - Usually you need to be logged in with the [Azure CLI](https://docs.microsoft.com/cli/azure/).
  - You also can be logged into Azure inside Visual Studio or Visual Studio Code.
- Azure Storage Account. Please follow this link for setting up the storage account: [Setup Storage Account](https://docs.microsoft.com/azure/storage/common/storage-account-create?tabs=azure-portal). 
- Azure Event Hub. Please follow this link for setting up the storage account: [Setup Event Hub](https://docs.microsoft.com/azure/event-hubs/event-hubs-create).
- You will need the Azure Event Hub Data Owner and Storage Blob Contributor role assigned to your account.

## Setup your environment

This sample needs to be configured with `Azure Event Hub` and `Azure Storage` to start. You can configure this either in Visual Studio or by using the command line.

### Command Line

Run this below command to set up the sample configuration in the dotnet secret store.

Open a command prompt and navigate to `samples\EventHubChannelSample` inside of this repository.
- For the command line to setup the secret store, you can run these commands:

```bash
dotnet user-secrets init 
dotnet user-secrets set "ConnectionString" "<<Your ConnectionString>>"
dotnet user-secrets set "HubName" "<<Name of your Event Hub>>"
dotnet user-secrets set "ExecutionStatusType" "<<Status Type>>"
dotnet user-secrets set "FallbackStorageConnectionString" "<<Your Fallback Storage Connection String>>" 
dotnet user-secrets set "FallbackStorageContainer" "<<Your Fallback Storage Container>>" 
dotnet user-secrets set "ProcessorStorageContainer" "<<Your Processor Storage Container>>"
```

### Visual Studio

If you are using Visual Studio, you can setup configuration via secrets without using the command line.

 1. Right-click on the EventHubChannelSample solution in the Solution Explorer and choose "Manage User Secrets".
 2. An editor for `secrets.json` will open. Paste the below inside of this file.

 ```json
  {
    "ConnectionString" :"<Your ConnectionString>",
    "HubName": "<Name of your Event Hub>",
    "ExecutionStatusType": "<Status Type>",
    "FallbackStorageConnectionString": "<Your Fallback Storage Connection String>",
    "FallbackStorageContainer": "<Your Fallback Storage Container>", 
    "ProcessorStorageContainer": "<Your Processor Storage Container>"
  }
```
3. Save and close `secrets.json`.

## Build the Sample 

- If you are using Microsoft Visual Studio, press Ctrl+Shift+B, or select Build > Build Solution 

- If you are using the .NET Core CLI, run the following command from the directory that contains this sample: 

```bash
dotnet build
```

## Run the Sample 

To debug the app and then run it, press F5 or use Debug > Start Debugging. To run the app without debugging, press Ctrl+F5 or use Debug > Start Without Debugging. 

- Using the .NET Core CLI 

    Run the following command from the directory that contains this sample: 

    ```bash
    dotnet run
    ```
## Usage Details 

- `Program.cs` file outlines how you can send the events to Azure Event Hub using Pipeline service. 
- Pipeline is a software design pattern that executes a sequence of operations, a pipeline consist of filters and/or channels.
- Channels are of two types: input channel and output channel which can be used as per our need. 
- For Azure Event Hub please refer to [this .NET documentation page](https://docs.microsoft.com/azure/event-hubs/event-hubs-about) for more information.
- **Option Pattern**: uses classes to provide strongly typed access to groups of related settings.Look at [this .NET documentation page](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-6.0) for more information.
- **Event Hub Channel**: the Channel that can send or receive data with Azure Event Hub. 
- **Event Hub Channel Options** - Event Hub channel options must be configured in order to send and receive data, below options need to configure for Azure Event Hub.
  - **Sku**- Azure Event Hub Sku used to determine the maximum      message size allowed by the Event Hub. 
  - **ConnectionString** - Azure Event Hub connection string.
  - **HubName** - Azure Event Hub used for this channel.
  - **ExecutionStatusType** - Gets or sets the requirement for   execution of the channel.
  - **FallbackStorageConnectionString**- Azure Blob Storage connection string used when data exceeds the allowable Azure Event Hub size.
  - **FallbackStorageContainer**- an Azure Blob Storage container used to store data when data exceeds the allowable Azure Event Hub size.
  - **ProcessorStorageContainer**- container used for managing the processor that reads messages.
- **UseWebPipeline** This is used to hook up our custom operation pipeline to ASP.NET.
- \<your service name\>.**OnReceive Event** that signals pipeline service has received a message.
- **SendMessageAsync** This method will execute the pipeline service which is configured in given Program.cs file. HttpRequestMessage is passed as an input parameter to the given method, which includes execution of the pipeline and sending message to the Azure Event Hub. Receive channel receives events from event hub. 
