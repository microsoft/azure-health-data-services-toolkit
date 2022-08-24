# Sending/Receiving Data with a Service Bus Channel

This sample shows you how you can *send and receive* data to/from Azure Service Bus using a channel in the Azure Health Data Services SDK. This is useful for notifying other systems of data in your pipelines or receiving data from other services.

## Concepts

- [Channels](/docs/concepts#channels) can be used as a source or a sink after your input or output filters in the pipeline.
- The Service Bus channel can be used as a source of data or a sink of data.
- Note: Receiving data only works for persistent hosting platforms like Azure App Service. It won't work for Azure Functions

## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.
- An authenticated Azure environment.
  - Usually you need to be logged in with the [Azure CLI](https://docs.microsoft.com/cli/azure/).
  - You also can be logged into Azure inside Visual Studio or Visual Studio Code.

## Setup your environment

You need to have an Azure subscription with an Azure Storage account to run this sample. Once you have this setup, create a blob container in the storage account. Copy the container name and generate a storage account level connection string. You also will need a Service Bus access key. These will need to be added as configuration for the sample. You can configure this either in Visual Studio or by using the command line.

**Visual Studio Code / Command Line**

Open a terminal command prompt and navigate to `samples/FeatureSamples/ServiceBusChannel` inside of this repository. Run the below to setup configuration with .NET user secrets.

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionString" "<<Your Service Bus Connection String>>"
dotnet user-secrets set "FallbackStorageConnectionString" "<<Your Fallback Storage Connection String>>"
dotnet user-secrets set "FallbackStorageContainer" "<<Your Fallback Storage Container Name>>"
dotnet user-secrets set "ExecutionStatusType" "<<Status type>>"
dotnet user-secrets set "Sku" "<<Name of your Service Bus SKU>>"
dotnet user-secrets set "Topic" "<<Name of your Service Bus Topic>>"
```

**Visual Studio**

You can add this configuration from inside of Visual Studio

1. Open the `ServiceBusChannelSample.sln` solution inside of Visual Studio.
2. Right-click on the ServiceBusChannelSample project in the Solution Explorer and choose "Manage User Secrets".
3. An editor for `secrets.json` will open. Paste the below inside of this file.

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

4. Save and close `secrets.json`.

## Build and run the sample

**Visual Studio Code**

From Visual Studio, you can click the "Debug" icon on the left and the play button to run and debug this sample.

**Visual Studio**

The easiest way to run the sample in Visual Studio is to use the debugger by pressing F5 or select "Debug > Start Debugging".

**Command Line**

From the command line, you can run the sample by executing `dotnet run` in this directory (`samples/FeatureSamples/ServiceBusChannel`).

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
