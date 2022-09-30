# Sending/Receiving Data with Event Hubs Channel

This sample shows you how you can *send and receive* data to/from Azure Event Hubs using a channel in the Azure Health Data Services toolkit. This is useful for notifying other systems of data in your pipelines or receiving data from other services.

## Concepts

- [Channels](/docs/concepts#channels) can be used as a source or a sink after your input or output filters in the pipeline.
- The Event Hub channel can be used as a source of data or a sink of data.
- Note: Receiving data only works for persistent hosting platforms like Azure App Service. It won't work for Azure Functions

## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.
- An authenticated Azure environment.
  - Usually you need to be logged in with the [Azure CLI](https://docs.microsoft.com/cli/azure/).
  - You also can be logged into Azure inside Visual Studio or Visual Studio Code.
- Azure Storage Account. Please follow this link for setting up the storage account: [Setup Storage Account](https://docs.microsoft.com/azure/storage/common/storage-account-create?tabs=azure-portal). 
- Azure Event Hub. Please follow this link for setting up the storage account: [Setup Event Hub](https://docs.microsoft.com/azure/event-hubs/event-hubs-create).

## Setup your environment

You need to have an Azure subscription with an Azure Storage account to run this sample. Once you have this setup, create a blob container in the storage account. Copy the container name and generate a storage account level connection string. You also will need a Event Hub access key. These will need to be added as configuration for the sample. You can configure this either in Visual Studio or by using the command line.

**Visual Studio Code / Command Line**

Open a terminal command prompt and navigate to `samples/FeatureSamples/EventHubChannel` inside of this repository. Run the below to setup configuration with .NET user secrets.

```bash
dotnet user-secrets init 
dotnet user-secrets set "ConnectionString" "<<Your ConnectionString>>"
dotnet user-secrets set "HubName" "<<Name of your Event Hub>>"
dotnet user-secrets set "ExecutionStatusType" "<<Status Type>>"
dotnet user-secrets set "FallbackStorageConnectionString" "<<Your Fallback Storage Connection String>>" 
dotnet user-secrets set "FallbackStorageContainer" "<<Your Fallback Storage Container>>" 
dotnet user-secrets set "ProcessorStorageContainer" "<<Your Processor Storage Container>>"
```

**Visual Studio**

You can add this configuration from inside of Visual Studio

1. Open the `EventHubChannelSample.sln` solution inside of Visual Studio.
2. Right-click on the EventHubChannelSample project in the Solution Explorer and choose "Manage User Secrets".
3. An editor for `secrets.json` will open. Paste the below inside of this file.

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

4. Save and close `secrets.json`.

## Build and run the sample

**Visual Studio Code**

From Visual Studio, you can click the "Debug" icon on the left and the play button to run and debug this sample.

**Visual Studio**

The easiest way to run the sample in Visual Studio is to use the debugger by pressing F5 or select "Debug > Start Debugging".

**Command Line**

From the command line, you can run the sample by executing `dotnet run` in this directory (`samples/FeatureSamples/EventHubChannel`).

## Usage details 

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
