# Simple External API Custom Operation 

This sample is a standard ASP.NET Web Application and will show how you can create a request object, pass it to the pipeline, and get the response back by using the SDK. 

This sample doesn't require any setup or any access to Azure Resources because it does everything locally.

## Concepts

This sample provides a way to configure the custom operation (CO) in the pipeline. You will implement the request and response with the use of a pipeline. 


To implement how requests and responses work, there are two applications:
1. SimpleCustomOperation: We are adding the simple inputs baseURL, method and path. 
2. SimpleWebApi: We are calling from the simplecustomoperation and returning the value (refer the simplecontroller).

## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.


## Build the Sample

You can configure this sample either in Visual Studio or by using the command line.

- If you are using Microsoft Visual Studio, press Ctrl+Shift+B, or select Build > Build Solution

- If you are using the .NET Core CLI, run the following command from the directory that contains this sample i.e. `samples/SimpleCustomOperation/SimpleCustomOperation` and `samples/SimpleCustomOperation/SimpleWebApi`: 

```bash
dotnet build
```

## Run the Sample

- To debug the app and then run it, press F5 or use Debug > Start Debugging. To run the app without debugging, press Ctrl+F5 or use Debug > Start Without Debugging. 

- Using the .NET Core CLI 

    Run the following command from the directory that contains this sample i.e. `samples/SimpleCustomOperation/SimpleCustomOperation` and `samples/SimpleCustomOperation/SimpleWebApi`:

    ```bash
    dotnet run
    ```

## Usage Details 

- Program.cs file  outlines how you can implement the SampleInputFilterOption and SampleOutputFilterOption. 

- An API Controller (CustomOperationController) outlines how you can implement IPipeline<HttpRequestMessage,HttpResponseMessage> interface for request and response. 

- Get: This method will demonstrate how we can call and send the value to another API and read the response.  

 

 
