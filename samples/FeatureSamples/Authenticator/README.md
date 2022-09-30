# Using Authenticator to access Azure resources

This sample will show you how you can get an access token for Azure resources with the toolkit. Custom operations and solutions built with this toolkit usually need access to Azure resources - from interacting with your FHIR service to integrations with Azure Storage or Service Bus.

Here, we'll cover different authentication methods and how to get tokens from Azure Active Directory which can be used to access any Azure service that supports Azure Active Directory authentication.

This sample is meant to be run locally on your computer, not deployed to Azure.

## Concepts

This sample provides easy configuration on top of the [Azure Identity Client Library for .NET](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme#defaultazurecredential). We follow best practices and allow you to use `DefaultAzureCredential`, which combines credentials commonly used to authenticate when deployed with credentials used to authenticate in a development environment. `DefaultAzureCredential` is intended to simplify getting started with the toolkit by handling common scenarios with reasonable default behaviors. Developers who want more control or whose scenario isn't served by the default settings can specify their authenticator method and settings.

## Prerequisites

- This repository cloned to your machine and an editor (e.g. Visual Studio or Visual Studio Code).
  - For Visual Studio, open the `Authenticator.csproj` project inside this directory (`samples/FeatureSamples/Authenticator`).
  - For other editors (like Visual Studio Code), open this directory (`samples/FeatureSamples/Authenticator`) in your editor.
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) downloaded and installed on your computer.
- An authenticated Azure environment.
  - Usually you need to be logged in with the [Azure CLI](https://docs.microsoft.com/cli/azure/).
  - You also can be logged into Azure inside Visual Studio or Visual Studio Code.
- A [FHIR Service deployed](https://docs.microsoft.com/azure/healthcare-apis/fhir/fhir-portal-quickstart) in your Azure environment to get a token for.
  - You will need the FHIR Contributor role assigned to your account.
- For the advanced scenario, you will need a [client application created in Azure Active Directory](https://docs.microsoft.com/azure/healthcare-apis/register-application).

## Setup your environment

First, make sure you have this sample open in your editor. Either open the solution file (`.sln`) in Visual Studio or ths sample folder in Visual Studio code.

This sample needs to be configured with only the `FhirServerUrl` to start. You can configure this either in Visual Studio or by using the command line.

**Visual Studio Code / Command Line**

Open a terminal or command prompt and navigate to `samples/FeatureSamples/Authenticator` inside of this repository. Run the below to setup configuration with .NET user secrets.

```bash
dotnet user-secrets init
dotnet user-secrets set "FhirServerUrl" "<Your-Fhir-Server-Url>"
```

**Visual Studio**

You can add this configuration from inside of Visual Studio

1. Right-click on the AuthenticatorSample project in the Solution Explorer and choose "Manage User Secrets".
2. An editor for `secrets.json` will open. Paste the below inside of this file.

    ```json
      {
        "FhirServerUrl": "<Your-Fhir-Server-Url>"
      }
    ```

3. Save and close `secrets.json`.

## Build and run the sample

**Visual Studio Code**

From Visual Studio, you can click the "Debug" icon on the left and the play button to run and debug this sample.

**Visual Studio**

The easiest way to run the sample in Visual Studio is to use the debugger by pressing F5 or select "Debug > Start Debugging".

**Command Line**

From the command line, you can run the sample by executing `dotnet run` in this directory (`samples/FeatureSamples/Authenticator`).

## Advanced scenario - specifying a client application

The Authenticator in the toolkit allows you to specify which authentication method your application will use. Here, let's set up this sample to use a client application (or service principal) to access Azure resources. You can actually achieve this in two ways:

- By [setting environment variables](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme#environment-variables).
- By using the built in configuration helpers inside this toolkit. We'll be doing this here.

To setup for service principal access using the build in configuration helpers, you'll need to add the below configuration.

For the command line, you can run these commands:

```bash
dotnet user-secrets set "TenantId" "<Your-Tenant-Id>"
dotnet user-secrets set "ClientId" "<Your-Client-Id>"
dotnet user-secrets set "ClientSecret" "<Your-Client-Secret>"
```

In Visual Studio, you can set your secrets file like below.

```json
{
  "FhirServerUrl": "<Your-Fhir-Server-Url>",
  "TenantId": "<Your-Tenant-Id>",
  "ClientId": "<Your-Client-Id>",
  "ClientSecret": "<Your-Client-Secret>"
}
```

Re-build and re-run the sample to ensure the new configuration is loaded.

## Sample structure details

- `Program.cs` is this sample is the entry point. This file sets up the sample.
- `MyService.cs` is a placeholder for a service where you would need to authenticate to access Azure resources.
- `MyConfig.cs` is an example configuration for the entire program. It uses the *option pattern* to provide strongly typed access to groups of related settings. Please refer to [this .NET documentation page](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme#environment-variables) for more information.
  - `MyServiceOptions.cs` is an example for configuration only for the placeholder service.