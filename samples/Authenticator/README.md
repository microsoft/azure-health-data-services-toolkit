---
page_type: sample
description: "Azure Health Data Services SDK sample showing how to get a token from Azure Active Directory."
languages:
- csharp
products:
- azure-health-data-services
---
# Sample Solution for Authenticator

A sample solution that describes how to generate the token using Azure Active Directory.

## Features

- The Purpose of this sample is to explain and demonstrate how to create token based on given Client Id, Tenant Id.
- This is helpful if you are planning to implement token-based authentication.

## Prerequisites

- Azure Subscription key.
- To understand what Azure Active Directory is and how you can configure and get the client id, tenant id, follow the below URLs:
  a. [How to create Tenant?](https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-access-create-new-tenant)
  b. [Create an Azure AD application and service principal that can access resources](https://docs.microsoft.com/azure/active-directory/develop/howto-create-service-principal-portal)
  c. [How to find Tenant Id?](https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-how-to-find-tenant)
  d. [FHIR Service](https://docs.microsoft.com/azure/healthcare-apis/fhir/get-started-with-fhir)
- Software Specification
  1. Microsoft Visual Studio 2017, Community Edition or higher.
  2. The .NET Core cross-platform development workload in Visual Studio. You can enable it in Tools > Get Tools and Features.
  3. .NET Core 6.0

## Set up the local Environment

- Download the sample the code.
- Run this below command to set up Environment variable locally,
  - Open a command prompt and navigate to the following folder,
    samples\ AuthenticatorSample.

    Run the following commands to setup the variables 
    1. dotnet user-secrets init
    2. dotnet user-secrets set “TenantId” “Your TenantId”
    3. dotnet user-secrets set “ClientId” “Your ClientId”
    4. dotnet user-secrets set “ClientSecret” “Your ClientSecret”
    5. dotnet user-secrets set “FhirServerUrl” “Your FhirServerUrl”

## Build the Sample

- If you are using Microsoft Visual Studio 2017 on Windows, press Ctrl+Shift+B, or select Build > Build Solution

- If you are using the .NET Core CLI, run the following command from the directory that contains this sample:
  dotnet build AuthenticatorSample/ AuthenticatorSample.csproj

## Run the Sample

- To debug the app and then run it, press F5 or use Debug > Start Debugging. To run the app without debugging, press Ctrl+F5 or use Debug > Start Without Debugging.

- Using the .NET Core CLI
  Run the following command from the directory that contains this sample:
  dotnet AuthenticatorSample\bin\Debug\net6.0 \ AuthenticatorSample.dll

## Usage Details

- Please check the Program.cs file that outlines how you can implement the authenticator logic and retrieve the token.
- **Option Pattern** uses classes to provide strongly typed access to groups of related settings. Please refer below URL for more understanding.

  [https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-6.0](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-6.0)

- UseAuthenticator: Using this extension method you can set the necessary parameter to authenticate the request.

- GetTokenAsync: Gets an access token via OAuth from Azure Active Directory.
