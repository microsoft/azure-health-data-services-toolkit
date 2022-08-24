# Injecting Custom Headers into your Custom Operations

This sample shows you how to work with the custom header utilities in the Azure Health Data Services SDK. We'll cover the three different types of headers modifications in this sample.

This sample doesn't contain a full custom operation pipeline, but is scoped to headers only.

This sample doesn't require any setup or any access to Azure Resources because it does everything locally.

## Concepts

This sample covers the following concepts:

- Header modification service configuration.
- Static headers which are always injected into the request.
- Request headers which are injected if the header exists on the request.
- Identity headers which inject a header from a claim in an bearer token.

Header modification is useful when you want to log information from the incoming request, especially using the [header logging capabilities](https://docs.microsoft.com//azure/healthcare-apis/azure-api-for-fhir/use-custom-headers) in the FHIR service.

## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- Open the `CustomHeaderSample.sln` solution file in Visual Studio or open `samples/FeatureSamples/CustomHeaders` folder in Visual Studio Code.
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) installed on your computer.

## Build and run the sample

**Visual Studio Code**

From Visual Studio, you can click the "Debug" icon on the left and the play button to run and debug this sample.

**Visual Studio**

The easiest way to run the sample in Visual Studio is to use the debugger by pressing F5 or select "Debug > Start Debugging".

**Command Line**

From the command line, you can run the sample by executing `dotnet run` in this directory (`samples/FeatureSamples/CustomHeaders`).

## Usage details

- **`Program.cs`**: Outlines how you can add configuration for custom headers. There are commends in this file - check it out.
- **Dependency Injection (DI)**: You should use dependency injection for `IHttpCustomHeaderCollection` in your custom operation pipeline filter. This will provide you the object you need to invoke the header replacement as defined in your `ConfigureServices` section in `Program.cs`.
- **`IHttpCustomHeaderCollection.AppendAndReplace`**: You will need to invoke this inside your `ExecuteAsync` method of your filter. This will actually perform the defined header modifications where you want in your pipeline.
