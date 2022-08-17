# Using In-Memory and Azure cache for Blob Storage

This sample will show you how you can access the Azure resources with the SDK. Custom operations and solutions built with this SDK usually need access to Azure resources - from interacting with your FHIR service to Azure Blob to store and cache the data.

Storing and reading the data from in-memory cache will improve the overall performance of the application.

Here, we'll cover how we can use in-memory caching technique methods and to cache the pipeline output data to Blob on Azure.

## Concepts

This sample provides easy configuration to cache the pipeline data to in-memory and on Azure Blob. This sample will show how to use `Azure Cache for Blob` and combines with in-memory caching techinque.

## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.
- An authenticated Azure environment.
  - Usually you need to be logged in with the [Azure CLI](https://docs.microsoft.com/cli/azure/).
  - You also can be logged into Azure inside Visual Studio or Visual Studio Code.
- You will need an Azure Storage account and Blob Container.

## Setup your environment

This sample needs to be configured with the 'Azure Storage Blob' connection string to start. You can configure this either in Visual Studio or by using the command line.

### Command Line

Run this below command to set up the sample configuration in the dotnet secret store.

- Open a command prompt and navigate to `samples\MemoryCacheAndBlobProvider` inside of this repository.

    ```bash
    dotnet user-secrets init
    dotnet user-secrets set "ConnectionString" "<<Your Blob Connection string>>"
    dotnet user-secrets set "Container" "<<Your Container Name>>"
    ```

### Visual Studio

If you are using Visual Studio, you can setup configuration via secrets without using the command line.

 1. Right-click on the MemoryCacheAndBlobProvider solution in the Solution Explorer and choose "Manage User Secrets".
 2. An editor for `secrets.json` will open. Paste the below inside of this file.

```json
  {
    "ConnectionString": "<Your Redis Connection string>",
    "Container":"<Your Container Name>" 
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

- To debug the app and then run it, press F5 or use Debug > Start Debugging. To run the app without debugging, press Ctrl+F5 or use Debug > Start Without Debugging. 

- Using the .NET Core CLI 

    Run the following command from the directory that contains this sample: 
    ```bash
    dotnet run
    ```

## Usage Details

- `Program.cs` file  outlines how you can implement caching, and create and store the file in Azure storage. 

- AddMemoryCache: Please refer to [this .NET documentation page](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.memorycacheservicecollectionextensions.addmemorycache?view=dotnet-plat-ext-6.0) for more information.

- **Option Pattern** uses classes to provide strongly typed access to groups of related settings. Look at [this .NET documentation page](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme#environment-variables) for more information.
 
- **SetAsync**:  This method internally calls IJsonObjectCache interface which is part of the SDK. It will create a record with the given key in Redis. 

- **GetAsync**: This method will read the data from Redis, DeserializeObject it. To read the data, you need to pass the key name as a parameter.