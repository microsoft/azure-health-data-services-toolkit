# Transforming Data with JsonTransform

This sample shows you how to work with the JSON transformation utilities in the Azure Health Data Services Toolkit. We'll show a simple example of how you can easily modify JSON data (usually HTTP request or response data) as part of a filter in your custom operations.

This sample doesn't contain a full custom operation pipeline, but is scoped to JSON transforms only.

This sample doesn't require any setup or any access to Azure Resources because it does everything locally.

## Concepts

This sample covers the following concepts:

- JSON `AddTransform` for appending data to JSON.
- JSON `TransformCollection` to list multiple transformations.

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

From the command line, you can run the sample by executing `dotnet run` in this directory (`samples/FeatureSamples/JsonTransform`).

## Usage details 

- `Program.cs` file outlines how you can add a node to the JSON data, transform using transform policy, and verify the results.
- **AddTransform** JSON transform used to add the node with properties like JSONPath, Append Node. 
- **TransformCollection**: This class represents a collection of JSON transforms used to add JSON transforms to the collection.  
- **TransformPolicy**: Uses transform collection and transforms JSON document into modified JSON data using Transform method, which is given below.
- \<Transform Policy name\>.**Transform** given method transforms a JSON data and returns the transformed document. JSON data to transform is passed as an input parameter for the given method, and the transformed document is returned as a JSON string.
- JSON result from above method is verified using JSON object, JArray and JToken.