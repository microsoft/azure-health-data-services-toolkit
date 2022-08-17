# Using of Custom headers

This sample solution describes how to append and replace existing headers with custom headers and returns the modified collection headers. 

## Concepts

- This sample demonstrates the Headers feature available in Azure Health Data Services SDK. given sample allows user to inject a new header name and value pair into existing header collection.  

- This is helpful if you are planning to append and replace existing headers with custom headers and return the modified collection headers. This is statically defined name and value of the header 

## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- Open the cloned repo project file in Visual Studio Or Open the clone repo folder in Visual Studio Code.
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.

## Build the Sample 

- If you are using Microsoft Visual Studio, press Ctrl+Shift+B, or select Build > Build Solution 

- If you are using the .NET Core CLI, run the following command from the directory that contains this sample: 

```bash
dotnet build
```

## Run the Sample 

To debug the app and then run it, press F5 or use Debug > Start Debugging. To run the app without debugging, press Ctrl+F5 or use Debug > Start Without Debugging. 

- Using the .NET Core CLI 

    Run the following command from the directory that contains this sample: 

    ```bash
    dotnet run
    ```

## Usage Details

- checkout the `Program.cs` file that outlines how you can implement the custom headers feature available in Azure Health Data Services SDK.
- **Dependency Injection (DI)** software design pattern, which is a technique for achieving Inversion of Control (IoC) between classes and their dependencies, Please refer to the URL below for more understanding.
[Dependency injection in ASP.NET Core | Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0)
- **GetCustomHeaders** This method internally calls AppendAndReplace method which is part of the SDK. It appends and replaces existing headers with custom headers and returns the modified collection headers. 
