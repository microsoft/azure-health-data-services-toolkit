# Sending Data to Event Grid in a Channel

This sample shows you how you can send data to Azure Event Grid using a channel in the Azure Health Data Services Toolkit. This is useful for notifying other systems of data in your pipelines.

## Concepts

- [Channels](/docs/concepts#channels) can be used as a source or a sink after your input or output filters in the pipeline.
- The Event Grid channel can only be used as a sink.

## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.
- An authenticated Azure environment.
  - Usually you need to be logged in with the [Azure CLI](https://docs.microsoft.com/cli/azure/).
  - You also can be logged into Azure inside Visual Studio or Visual Studio Code.
- Azure Storage account. Please follow this link for setting up the storage account: [Setup Storage Account](https://docs.microsoft.com/azure/storage/common/storage-account-create?tabs=azure-portal).
- Azure Event Grid. Please follow this link for setting up the Event Grid: [Setup Event Grid](https://docs.microsoft.com/azure/event-grid/create-view-manage-system-topics).

## Setup your environment

You need to have an Azure subscription with an Azure Storage account to run this sample. Once you have this setup, create a blob container in the storage account. Copy the container name and generate a storage account level connection string. You also will need a topic setup for Event Grid and the access key. These will need to be added as configuration for the sample. You can configure this either in Visual Studio or by using the command line.

**Visual Studio Code / Command Line**

Open a terminal command prompt and navigate to `samples/FeatureSamples/EventGridChannelSample` inside of this repository. Run the below to setup configuration with .NET user secrets.

```bash
dotnet user-secrets init 
dotnet user-secrets set "Subject" "<<Your Subject>>" 
dotnet user-secrets set "AccessKey" "<<Your Topic Access Key>>"
dotnet user-secrets set "ExecutionStatusType" "Any"
dotnet user-secrets set "DataVersion" "<<Your Event Data Version>>" 
dotnet user-secrets set "EventType" "Test" 
dotnet user-secrets set "FallbackStorageConnectionString" "<<Your Fallback Storage Connection String>>"
dotnet user-secrets set "FallbackStorageContainer" "<<Your Fallback Storage Container>>"
dotnet user-secrets set "TopicUriString" "<<Your Topic Uri String>>"
```

**Visual Studio**

You can add this configuration from inside of Visual Studio

1. Open the `EventGridChannelSample.sln` solution inside of Visual Studio.
2. Right-click on the EventGridChannelSample project in the Solution Explorer and choose "Manage User Secrets".
3. An editor for `secrets.json` will open. Paste the below inside of this file.

    ```json
      {
        "Subject": "<Your Subject>",
        "AccessKey": "<Your Topic Access Key>",
        "ExecutionStatusType": "<Your Execution Status Type>",
        "DataVersion": "<Your Event Data Version>", 
        "EventType": "<Your Event Type>",  
        "FallbackStorageConnectionString": "<You Fallback Storage Connection String>", 
        "FallbackStorageContainer": "<Your Fallback Storage Container>", 
        "TopicUriString": "<Your Topic Uri String>" 
      }
    ```

4. Save and close `secrets.json`.

## Build and run the sample

**Visual Studio Code**

From Visual Studio, you can click the "Debug" icon on the left and the play button to run and debug this sample.

**Visual Studio**

The easiest way to run the sample in Visual Studio is to use the debugger by pressing F5 or select "Debug > Start Debugging".

**Command Line**

From the command line, you can run the sample by executing `dotnet run` in this directory (`samples/FeatureSamples/EventGridChannel`).

## Usage details

- **`Program.cs`**: Outlines how you can add configuration for a Event Grid Channel. There are commends in this file - check it out.
- **`PipelineService.cs`/`IPipelineService.cs`**: Provide access to test the web pipeline for the sample.
- For **Azure Event Grid** please refer to [this .NET documentation page](https://docs.microsoft.com/azure/event-grid/overview) for more information.
- **Option Pattern**: uses classes to provide strongly typed access to groups of related settings. Please refer to [this .NET documentation page](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme#environment-variables) for more information.
- **Event Grid Channel**: the Channel used to send events to Azure event grid. 
- **Event Grid Channel Options**: Event Grid channel options must be configured in order to send data; below options need to be configured for Azure Event Grid. 

  - **TopicUriString**- Gets or sets the Azure Event Grid topic. 

  - **AccessKey**- Gets or sets the Azure Event Grid access key. 

  - **Subject**- Gets or sets Azure Event Grid subject. 

  - **EventType**- Gets or sets the Azure Event Grid event type. 

  - **DataVersion**- Gets or sets the Azure Event Grid data version.  

  - **ExecutionStatusType** Gets or sets the requirement for execution of the channel. 

  - **FallbackStorageConnectionString**- Gets or sets an Azure Blob Storage connection string used when data exceeds the allowable Azure Event Grid size. 

   - **FallbackStorageContainer**- Gets or sets an Azure Blob Storage container used to store data when data exceeds the allowable Azure Event Grid size.
- **UseWebPipeline** - **Azure Queue Storage** is used when data exceeds the allowable Azure Event Grid Size. Blob storage is used for Queue storage.
- \< your service name \>**.OnReceive** Event that signals pipeline service has received a message.
- **ExecuteAsync** This method executes the pipeline service which is configured in given Program.cs file and HttpRequestMessage is passed as an input parameter to the given method, it includes execution of the pipeline and sending message to the Azure Event Grid. 
