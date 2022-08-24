# Blob Storage Output Channel

This samples shows you how to setup an output channel to Azure Blob Storage. You would use this if you needed to output data (like the request or response) in your custom operation.

## Concepts

- [Channels](/docs/concepts#channels) can be used as a source or a sink after your input or output filters in the pipeline.
- The Blob Storage channel can only be used as a sink.

## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.
- An authenticated Azure environment.
  - Usually you need to be logged in with the [Azure CLI](https://docs.microsoft.com/cli/azure/).
  - You also can be logged into Azure inside Visual Studio or Visual Studio Code.
- Azure Storage account. Please follow this link for setting up the storage account: [Setup Storage Account](https://docs.microsoft.com/azure/storage/common/storage-account-create?tabs=azure-portal).

## Setup your environment

You need to have an Azure subscription with an Azure Storage account and Event Grid to run this sample. Once you have this setup, create a blob container in the storage account. Copy the container name and generate a storage account level connection string. These will need to be added as configuration for the sample. You can configure this either in Visual Studio or by using the command line.

**Visual Studio Code / Command Line**

Open a terminal or command prompt and navigate to `samples\FeatureSamples\BlobChannelSample` inside of this repository. Run the below to setup configuration with .NET user secrets.

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionString" "<<Your Storage Account Connection String>>"
dotnet user-secrets set "Container" "<<Your Storage Account Container Name>>"
```

**Visual Studio**

You can add this configuration from inside of Visual Studio

1. Open the `BlobChannelSample.sln` solution inside of Visual Studio.
2. Right-click on the BlobChannelSample project in the Solution Explorer and choose "Manage User Secrets".
3. An editor for `secrets.json` will open. Paste the below inside of this file.

    ```json
      {
        "ConnectionString": "<Your Storage Account Connection String>",
        "Container": "<Your Storage Account Container Name>"
      }
    ```

4. Save and close `secrets.json`.

## Build and run the sample

**Visual Studio Code**

From Visual Studio, you can click the "Debug" icon on the left and the play button to run and debug this sample.

**Visual Studio**

The easiest way to run the sample in Visual Studio is to use the debugger by pressing F5 or select "Debug > Start Debugging".

**Command Line**

From the command line, you can run the sample by executing `dotnet run` in this directory (`samples/FeatureSamples/BlobChannel`).

## Usage details

- `Program.cs`: Outlines how you can add configuration for a Blob Channel. There are commends in this file - check it out.
- `PipelineService.cs`/`IPipelineService.cs` provide access to test the web pipeline for the sample.
- `BlobChannelConfig.cs` is an example configuration for the entire program. It uses the *option pattern* to provide strongly typed access to groups of related settings. Please refer to [this .NET documentation page](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme#environment-variables) for more information.
- `RandomString.cs` provides some sample data.
