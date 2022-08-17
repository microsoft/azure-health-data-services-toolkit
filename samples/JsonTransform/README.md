# Using JsonTransform to Transform the Data

This sample will show you how you can read  JSON data and transform it using various methods provided by the SDK.

Here we'll cover the scenario which can be used to read the data from a JSON file. 

## Concepts

This sample will help you to understand how to use JSON features for JSON data transform with this SDK. This sample will show how to read and transform JSON data. 

## Prerequisites

- This repository cloned to your machine and an editor. (e.g. Visual Studio or Visual Studio Code). 
- Open the cloned repo project file in Visual Studio or open the clone repo folder in Visual Studio Code. 
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
- `Program.cs` file outlines how you can add a node to the JSON data, transform using transform policy, and verify the results.
- **AddTransform** JSON transform used to add the node with properties like JSONPath, Append Node. 
- **TransformCollection**: This class represents a collection of JSON transforms used to add JSON transforms to the collection.  
- **TransformPolicy**: Uses transform collection and transforms JSON document into modified JSON data using Transform method, which is given below. 
- \<Transform Policy name\>.**Transfrom** given method transforms a JSON data and returns the transformed document. JSON data to transform is passed as an input parameter for the given method, and the transformed document is returned as a JSON string.  
- JSON Result from above method is verified using JSON object, JArray and JToken. 

 
