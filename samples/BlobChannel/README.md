# Using Blob storage as Channel

In this sample, a pipeline is created with a Blob channel to send pipeline data to Azure Blob Storage. 

## Concepts

This sample shows how to configure a channel to send pipeline data to Azure Blob storage. 

## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.
- An authenticated Azure environment.
  - Usually you need to be logged in with the [Azure CLI](https://docs.microsoft.com/cli/azure/).
  - You also can be logged into Azure inside Visual Studio or Visual Studio Code.
- You will need the Storage Blob Contributor role assigned to your Azure account.

## Setup your environment

This sample needs to be configured with the variables mentioned below to start. You can configure this either in Visual Studio or by using the command line.

### Command Line

Run this below command to set up the sample configuration in the dotnet secret store.

Open a command prompt and navigate to `samples\BlobChannelSample` inside of this repository.
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionString" "<<Your Storage Account Connection String>>"
dotnet user-secrets set "Container" "<<Your Storage Account Container Name>>"
```

### Visual Studio

If you are using Visual Studio, you can setup configuration via secrets without using the command line.

 1. Right-click on the BlobChannelSample solution in the Solution Explorer and choose "Manage User Secrets".
 2. An editor for `secrets.json` will open. Paste the below inside of this file.

```json
  {
    "ConnectionString": "<Your Storage Account Connection String>",
    "Container": "<Your Storage Account Container Name>"
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

    Navigate to `samples\BlobChannelSample` directory and run the following command:

    ```bash
    dotnet run
    ```

## Usage Details

- `Program.cs` file in the sample outlines how you can implement the pipeline, channel features, and how to store and fetch the output data from Azure Storage.
- Pipelines are used to build the `custom operations` and can be used to 
  - modify information 
  - acquire additional information to make decisions
  - output information to our services
  
  The first two are performed through a chain of 0 or more `filters` where each filter in the chain performs some operation.  The latter is performed through `channels`, which simply output information 0 or more desired services.
- **Option Pattern** uses classes to provide strongly typed access to groups of related settings. Please refer to [this .NET documentation page](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme#environment-variables) for more information.
- **UseWebPipeline**: This is used to hook up our custom operation pipeline to ASP.NET.
- **ExecuteAsync**: This method internally calls ExecuteChannelsAsync method which is part of the Azure Health Data Services SDK. It executes the pipeline and channel and returns a response for the caller.
