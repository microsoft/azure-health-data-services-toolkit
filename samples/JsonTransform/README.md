# Using JsonTransform to Transform the Data

This sample will show you how you can read the json data and transform it using various method provided by SDK.

Here We'll cover the scenario which can be used to read the data from the Json. 

## Concepts

This sample will help you to understand how to use Json features for Json data transform with this SDK. We follow the best practices and allow you to understand how to read and transform the data. 

## Prerequisites

- This repository cloned to your machine and an editor. (e.g. Visual Studio or Visual Studio Code). 
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

- CheckOut  Program.cs file that outlines how you can add the node to the Json data and transform using transform policy and verify the results.
- AddTransform Json transform used to add the node with properties like JsonPath, Append Node. 
- TransformCollection this class represents a collection of Json transforms used to add Json transforms to the collection.  
- TransformPolicy uses transform collection and transforms Json document into modified Json data using Transform method which is given below. 
- .Transfrom given method Transforms a Json data and returns the transformed document, Json data to transform is passed as an input parameter for the given method and it returns transformed document as a Json string.  
- Json Result from above method is verified using Json object, JArray and JToken. 

 
