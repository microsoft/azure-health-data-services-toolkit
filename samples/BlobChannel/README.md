# Using Blob storage as Channel

This sample demonstrates the channel feature available in Azure Health Data Services SDK. In this sample, pipeline is created and a channel feature is used to store pipeline data to Azure storage account.
## Concepts
This sample provides easy configuration of the application with Azure Blob storage and Azure storage with this SDK. We follow the best practices and allow you to understand how to configure the Blob storage as a channel and its options using Input channel and how we can use blob for storing pipline data.
## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.
- An authenticated Azure environment.
  - Usually you need to be logged in with the [Azure CLI](https://docs.microsoft.com/cli/azure/).
  - You also can be logged into Azure inside Visual Studio or Visual Studio Code.
- You will need the Storage Blob Contributor role assigned to your Azure account.

## Setup your environment

This sample needs can be configured with varibles mentioned below to start. You can configure this either in Visual Studio or by using the command line.

### Command Line

Run this below command to set up the sample configuration in the dotnet secret store.

Open a command prompt and navigate to `samples\BlobChannelSample` inside of this repository.
```bash
dotnet user-secrets init
dotnet user-secrets set “TenantId” “<<Your Tenant ID>>”
dotnet user-secrets set “ConnectionString” “<<Your Storage Account Connection String>>”
dotnet user-secrets set “Container” “<<Your Storage Account Container Name>>”
```

### Visual Studio

If you are using Visual Studio, you can setup configuration via secrets without using the command line.

 1. Right-click on the BlobChannelSample solution in the Solution Explorer and choose "Manage User Secrets".
 2. An editor for `secrets.json` will open. Paste the below inside of this file.

```json
  {
    "TenantId": "<Your Tenant ID>",
    "ConnectionString": "<Your Storage Account Connection String>",
    "Container": "<Your Storage Account Container Name>"
  }
```

3. Save and close `secrets.json`.

## Build the Sample 

- If you are using Microsoft Visual Studio 2017 on Windows, press Ctrl+Shift+B, or select Build > Build Solution 

- If you are using the .NET Core CLI, run the following command from the directory that contains this sample: 

```bash
dotnet build BlobChannelSample/BlobChannelSample.csproj 
```

## Run the Sample 

To debug the app and then run it, press F5 or use Debug > Start Debugging. To run the app without debugging, press Ctrl+F5 or use Debug > Start Without Debugging. 

- Using the .NET Core CLI 

    Run the following command from the directory that contains this sample: 

    ```bash
    dotnet BlobChannelSample\bin\Debug\net6.0\BlobChannelSample.dll
    ```

## Usage Details

- checkout the `Program.cs` file in the sample that outlines how you can implement the pipeline, channel features and store and fetch the output data from Azure storage.
- Pipelines are used to build the `custom operations` and can be used to 
  - modify information 
  - acquire additional information to make decisions
  - output information to our services
  
  The first two are performed through a chain of 0 or more `filters` where each filter in the chain performs some operation.  The later is performed through `channels`, which simply output information 0 or more desired services.
- **Option Pattern** uses classes to provide strongly typed access to groups of related settings. Look at [this .NET documentation page](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme#environment-variables) for more information.
- **UseWebPipeline** This is used to add the scoped to the web services configuration.
- **ExecuteAsync**  This method internally calls ExecuteChannelsAsync method which is part of the Azure Health Data Services SDK. It executes the pipeline and Channel and returns a response for the caller.
