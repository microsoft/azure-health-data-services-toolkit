# Azure Function custom operation quickstart

This quickstart will walk you through creating a simple custom operation on top of the FHIR Service using Azure Functions. We'll cover everything from deploying infrastructure, debugging locally, and deploying to Azure.

*This sample does not address authorization for simplicity - the endpoint it open to anyone with the address. Please only use test or sample data for this quickstart.*

## Prerequisites

- An Azure account with an active subscription.
  - You need access to create resource groups, resources, and role assignments in Azure
- [.NET 6.0](https://dotnet.microsoft.com/download)
- [Azure Command-Line Interface (CLI)](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Azure Developer CLI](https://docs.microsoft.com/azure/developer/azure-developer-cli/get-started?tabs=bare-metal%2Cwindows&pivots=programming-language-csharp#prerequisites)
- Visual Studio or Visual Studio Code
  - For Visual Studio, you will need the Azure Functions Tools. To add Azure Function Tools, include the Azure development workload in your Visual Studio installation.
  - For Visual Studio Code, you will need to install the [Azure Function Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash#install-the-azure-functions-core-tools).

### Prerequisite check

- In a terminal or command window, run `dotnet --version` to check that the .NET SDK is version 6.0 or later.
- Run `az --version` and `azd --version` to check that you have the appropriate Azure command-line tools installed.

## Setting up

This quickstart will create the below resources. These will be used both for local development and the Azure deployment.

- Azure Health Data Services workspace
- FHIR Service
- Function App (and associated storage account)
- Log Analytics Workspace (for FHIR Service and Function App logs)
- Application Insights (for monitoring your custom operation)

### Deploy Quickstart

1. Deploy the needed resources with `azd`. 

    ```dotnetcli
    azd up --template Azure-Samples/ahds-sdk-fhir-function-quickstart
    ```

2. This will take about 20 minutes to deploy the FHIR Service.
    a. `If you have run this sample in the past, using the same environment name and location will reuse your previous resources.`

## Testing locally

### Visual Studio Code

1. Open this folder in Visual Studio Code (`samples/Quickstart`).
2. You may be asked to install recommended extensions for the repository. Click "Yes" to install the needed tools
    1. Relaunch Visual Studio Code if this is your first time working with the Azure Function Tools.
3. Start the Azurite emulator by clicking `Azurite Blob Service` in the bottom blue bar or selecting `Azurite: Start` from the command palate.
4. Start the Quickstart function app by going to "Run and Debug" and selecting the play button (or hit F5 on your keyboard).
5. You can now test your code locally! Set a breakpoint and go to `http://localhost:7071/Patient` in your browser or API testing tool.

### Visual Studio

1. Open the `Quickstart.sln` project inside of Visual Studio.
2. Debug the custom operation inside of Visual Studio.
3. You can now test your code locally! Set a breakpoint and go to `http://localhost:7256/Patient` in your browser or API testing tool.

## Deploying to Azure

1. Once you are ready to deploy to Azure, we can use azd. Run `azd deploy` from your terminal or command prompt.
2. The command will output ae endpoint for your function app. Copy this.
3. Test the endpoint by going to `<Endpoint>/Patient` in your browser or API testing tool.
