# Custom Operation 

This sample will show you how you can create a request object and pass it to the pipeline and get the response back by using the SDK. 

## Concepts

This sample provides way to configure the custom operation (CO) in the pipeline. You will implement the request and response with the use of Pipeline. 

To implement how requests and responses work, there are two applications:
1. SimpleCustomOperation: We are adding the simple inputs baseURL, method and path. 
2. SimpleWebApi: We are calling from the simplecustomoperation and return the value (refer the simplecontroller).

## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.

## Build the Sample

You can configure this sample either in Visual Studio or by using the command line.

- If you are using Microsoft Visual Studio 2017 on Windows, press Ctrl+Shift+B, or select Build > Build Solution

- If you are using the .NET Core CLI, run the following command from the directory that contains this sample: 

```bash
dotnet build SimpleCustomOperation\SimpleCustomOperation.csproj 

dotnet build SimpleWebApi\SimpleWebApi.csproj 
```

## Run the Sample

- To debug the app and then run it, press F5 or use Debug > Start Debugging. To run the app without debugging, press Ctrl+F5 or use Debug > Start Without Debugging. 

- Using the .NET Core CLI 

    Run the following command from the directory that contains this sample: 

    ```bash
    dotnet SimpleCustomOperation\bin\Debug\net6.0\SimpleCustomOperation.dll 

    dotnet SimpleWebApi\bin\Debug\net6.0\SimpleWebAPI.dll 
    ```

## Usage Details 

- Checkout the Program.cs file that outlines how you can implement the SampleInputFilterOption and SampleOutputFilterOption. 

- An API Controller (CustomOperationController) which outline how you can implement IPipeline<HttpRequestMessage,HttpResponseMessage> interface for request and response. 

- Get: This method will demonstrate how we can call and send the value to another API and read the response.  

 

 