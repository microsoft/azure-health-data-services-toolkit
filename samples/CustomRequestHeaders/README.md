# Using Custom Request Header

This sample solution describes how to append and replace existing headers with custom headers by determining the value in an incoming http request and returns the modified collection headers. 

## Concepts

- This sample demonstrates the feature Custom Headers available in Azure Health Data Services SDK. given sample allows user to inject a new header name with the value determined by an incoming http request.  

- sample is helpful if you are planning to append and replace existing headers with custom headers and return the modified collection headers. The `name` parameter is the name of the new http header, the  value is a name of a header in an incoming http request value is used in the new header.

## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.

## Setup your environment

- Make sure that sample code repository cloned and loaded properly in a editor (e.g. Visual Studio or Visual Studio Code).


## Build the Sample 

- If you are using Microsoft Visual Studio, press Ctrl+Shift+B, or select Build > Build Solution 

- If you are using the .NET Core CLI, run the following command from the directory that contains this sample: 

```json
dotnet build
```

## Run the Sample 

To debug the app and then run it, press F5 or use Debug > Start Debugging. To run the app without debugging, press Ctrl+F5 or use Debug > Start Without Debugging. 

- Using the .NET Core CLI 

    Run the following command from the directory that contains this sample: 

    ```json
    dotnet run
    ```

## Usage Details

- checkout the `Program.cs` file that outlines how you can implement the custom headers feature available in Azure Health Data Services SDK.
- **Dependency Injection (DI)** software design pattern, which is a technique for achieving Inversion of Control (IoC) between classes and their dependencies, Please refer to the URL below for more understanding.
[Dependency injection in ASP.NET Core | Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0)
- **GetCustomHeaders** This method internally calls AppendAndReplace method which is part of the SDK. It appends and replaces existing headers with custom headers and returns the modified collection headers. 