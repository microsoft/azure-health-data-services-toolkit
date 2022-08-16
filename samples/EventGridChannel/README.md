# Using Azure Event Grid as channel
This sample demonstrates the use of Azure Event Grid as a channel which is available in Azure Health Data Services SDK. we'll cover channel scenario which can be used to send events to Azure event grid.

## Concepts
This sample will help you to understand how to configure the event grid channel and its options using input channel and how to send the events to Azure event grid and use Blob storage as a queue storage for Azure Event Grid.  
## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.
- An authenticated Azure environment.
  - Usually you need to be logged in with the [Azure CLI](https://docs.microsoft.com/cli/azure/).
  - You also can be logged into Azure inside Visual Studio or Visual Studio Code.
- You will need the Event Grid Contributor Role and Storage Blob Contributor role assigned to your account.

## Setup your environment

This sample needs can be configured with varibles mentioned below to start. You can configure this either in Visual Studio or by using the command line.

### Command Line

Run this below command to set up the sample configuration in the dotnet secret store.

Open a command prompt and navigate to `samples\EventGridChannelSample` inside of this repository.

- For the command line to setup the secret store, you can run these commands:

```bash
dotnet user-secrets init 
dotnet user-secrets set "Subject" "<<Your Subject>>" 
dotnet user-secrets set "AccessKey" "<<Your Topic Access Key>>"
dotnet user-secrets set "ExecutionStatusType" 
"<<Your Execution Status Type>>"
dotnet user-secrets set "DataVersion" "<<Your Event Data Version>>" 
dotnet user-secrets set "EventType" "<<Your Event Type>>" 
dotnet user-secrets set "FallbackStorageConnectionString" "<<Your Fallback Storage Connection String>>"
dotnet user-secrets set "FallbackStorageContainer" "<<Your Fallback Storage Container>>"
dotnet user-secrets set "TopicUriString" "<<Your Topic Uri String>>"
```

### Visual Studio

If you are using Visual Studio, you can setup configuration via secrets without using the command line.

 1. Right-click on the BlobChannelSample solution in the Solution Explorer and choose "Manage User Secrets".
 2. An editor for `secrets.json` will open. Paste the below inside of this file.

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

3. Save and close `secrets.json`.

## Build the Sample 

- If you are using Microsoft Visual Studio 2017 on Windows, press Ctrl+Shift+B, or select Build > Build Solution 

- If you are using the .NET Core CLI, run the following command from the directory that contains this sample: 

```bash
dotnet build EventGridChannelSample/EventGridChannelSample.csproj 
```

## Run the Sample 

To debug the app and then run it, press F5 or use Debug > Start Debugging. To run the app without debugging, press Ctrl+F5 or use Debug > Start Without Debugging. 

- Using the .NET Core CLI 

    Run the following command from the directory that contains this sample: 

    ```bash
    dotnet EventGridChannelSample\bin\Debug\net6.0 \EventGridChannelSample.dll
    ```

## Usage Details

- Please Refer the Program.cs file that outlines how you can send the events to Azure event grid using Pipeline service. 

- Pipeline is a software design pattern that executes a sequence of operations, pipeline consist of filters or channels. 

- Channels are of two types, input channel and output channel which can be used as per our need. 
- For understanding Azure Event Grid look at [this .NET documentation page](https://docs.microsoft.com/en-us/azure/event-grid/overview) for more information.

- **Option Pattern** uses classes to provide strongly typed access to groups of related settings. Look at [this .NET documentation page](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme#environment-variables) for more information.
- **Event Grid Channel** the Channel used to send events to Azure event grid. 
- Event Grid Channel Options Event Grid channel options must be configured in order to send data; below options need to configure for Azure Event Grid. 

  - TopicUriString- Gets or sets the Azure Event Grid topic. 

  - AccessKey- Gets or sets the Azure Event Grid access key. 

  - Subject- Gets or sets Azure Event Grid subject. 

  - EventType- Gets or sets the Azure Event Grid event type. 

  - DataVersion- Gets or sets the Azure Event Grid data version.  

  - ExecutionStatusType Gets or sets the requirement for execution of the channel. 

  - FallbackStorageConnectionString- Gets or sets an Azure Blob Storage connection string used when data exceeds the allowable Azure Event Grid size. 

   - FallbackStorageContainer- Gets or sets an Azure Blob Storage container used to store data when data exceeds the allowable Azure Event Grid size.
- **UseWebPipeline** given extension is used to add scope to the web services configurations.
- **Azure Queue Storage** is used when data exceeds the allowable Azure Event Grid Size, Blob storage is used for Queue storage.
- **< your service name >.OnReceive"** Event that signals pipeline service has received a message.
- **ExecuteAsync** This method Executes the pipeline service which is configured in given Program.cs file and HttpRequestMessage is passed as an input parameter to the given method, it includes execution of the pipeline and sending message to the Azure event grid. 