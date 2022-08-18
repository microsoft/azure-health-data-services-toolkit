# Channel to Azure Service Bus

In this sample, a pipeline is created with a channel to send data to Azure Service Bus.

## Concepts

This sample shows how to configure a channel to send pipeline data to Azure Service Bus.

## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.
- An authenticated Azure environment.
  - Usually you need to be logged in with the [Azure CLI](https://docs.microsoft.com/cli/azure/).
  - You also can be logged into Azure inside Visual Studio or Visual Studio Code.
- You will need the Azure Service Bus Data Owner and Storage Blob Contributor role assigned to your account.

## Setup your environment

This sample needs to be configured with `Azure service Bus` and `Azure Storage` to start. You can configure this either in Visual Studio or by using the command line.

### Command line

Run this below command to set up the sample configuration in the dotnet secret store.

Open a command prompt and navigate to `samples\ServiceBusChannelSample` inside of this repository.
- For the command line to setup the secret store, you can run these commands:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionString" "<<Your Service Bus Connection String>>"
dotnet user-secrets set "FallbackStorageConnectionString" "<<Your Fallback Storage Connection String>>"
dotnet user-secrets set "FallbackStorageContainer" "<<Your Fallback Storage Container Name>>"
dotnet user-secrets set "ExecutionStatusType" "<<Status type>>"
dotnet user-secrets set "Sku" "<<Name of your Service Bus SKU>>"
dotnet user-secrets set "Topic" "<<Name of your Service Bus Topic>>"
```

### Visual Studio

If you are using Visual Studio, you can setup configuration via secrets without using the command line.

 1. Right-click on the ServiceBusChannelSample solution in the Solution Explorer and choose "Manage User Secrets".
 2. An editor for `secrets.json` will open. Paste the below inside of this file.

```json
  {
    "ConnectionString": "<<Your Service Bus Connection String>>",
    "FallbackStorageConnectionString": "<<Your Fallback Storage Connection String>>",
    "FallbackStorageContainer": "<<Your Fallback Storage Container Name>>",
    "ExecutionStatusType": "<<Status type>>",
    "Sku": "<<Name of your Service Bus SKU>>",
    "Topic": "<<Name of your Service Bus Topic>>"
  }
```

3. Save and close `secrets.json`.

## Build the sample 

- If you are using Microsoft Visual Studio, press Ctrl+Shift+B, or select Build > Build Solution 

- If you are using the .NET Core CLI, run the following command from the directory that contains this sample: 

```bash
dotnet build
```

## Run the sample 

To debug the app and then run it, press F5 or use Debug > Start Debugging. To run the app without debugging, press Ctrl+F5 or use Debug > Start Without Debugging. 

- Using the .NET Core CLI 

    Run the following command from the directory that contains this sample: 

    ```bash
    dotnet run
    ```

## Usage details 

- Program.cs file  outlines how you can send and receive messages with Service Bus using Pipeline service. 
- Pipeline is a software design pattern that executes a sequence of operations, pipeline consist of filters or channels. 
- Channels are of two type’s input channel and output channel which can be used as per our need. 
- For understanding Azure Service Bus please refer to [this .NET documentation page](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview ) for more information. 
- **Option Pattern** uses classes to provide strongly typed access to groups of related settings. Look at [this .NET documentation page](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-6.0) for more information
- **Service Bus Channel**: the Channel that can send or receive data with Azure Service Bus. 
- **Service Bus Channel Options**:N Service Bus channel options must be configured in order to send and receive data, below options need to configure for Azure Service Bus. 
    - **ConnectionString** - Azure Service Bus connection string 
    - **FallbackStorageConnectionString**- Blob storage connection string used when storing large messages. 
    - **FallbackStorageContainer**- Blob storage container used to store large messages. 
    - **ExecutionStatusType** - Gets or sets the requirement for execution of the channel. 
    - **Sku**- type of Service Bus sku used. 
    - **Topic**- Service Bus topic. 

- **UseWebPipeline**: his is used to hook up our custom operation pipeline to ASP.NET.![image](https://user-images.githubusercontent.com/33711652/185283884-ddccdff1-33a8-4900-9213-4c0207a7e81a.png)

- **OnReceive**: Event that signals pipeline service has received a message. 
- **SendMessageAsync**: This method will execute the pipeline service which is configured in Program.cs file and HttpRequestMessage is passed as an input parameter to the given method, it includes execution of the pipeline and sending message to the Azure Service Bus and Receive channel receives events from Service Bus.
