# Azure Health Data Services SDK Azure Function Custom Operation Quickstart

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
- Run `az --version` and `azd version` to check that you have the appropriate Azure command-line tools installed.

## Setting up

This quickstart will create the below resources. These will be used both for local development and the Azure deployment.

- Azure Health Data Services workspace
- FHIR Service
- Function App (and associated storage account)
- Log Analytics Workspace (for FHIR Service and Function App logs)
- Application Insights (for monitoring your custom operation)

### Deploy Quickstart

1. Create a new directory on your local machine and open that directory in a terminal or command prompt.
2. User need to be logged in to Azure using `az login` .
3. Deploy the needed resources with the below `azd` command. This will pull the Quickstart code and deploy needed Azure resources.

    ```dotnetcli
    azd up --template Azure-Samples/ahds-sdk-fhir-function-quickstart
    ```

4. This will take about 20 minutes to deploy the FHIR Service.
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

## Usage details

- `Program.cs` outlines how we can use Azure Function for Simple custom operation using various types of services like authenticator, headers and filters.
    - UseAuthenticator() Used for accessing Azure resources, it uses azure default credentials.
    - UseCustomHeaders()  Used  for custom headers Setup, using this service we can add custom header, here we have added custom header with name `X-MS-AZUREFHIR-AUDIT-USER-TOKEN-TEST`.
    - UseAzureFunctionPipeline() setup pipeline for Azure function.
    - AddInputFilter(typeof(QuickstartFilter)) Input filter added with name `QuickStart` which in turn used to modify the patient data using JsonTransform.
    -  Add binding to pass the call to the FHIR service.
- Please refer `QuickstartFilter.cs` file for input filter modifications in the Patient Data.
   - Added language to resource as ‘en’ (English)
    - If there is no `Patient.meta.security` label, added [HTEST](https://www.hl7.org/fhir/resource-definitions.html#Meta.security)
- Custom operation QuickstartSample end point methods listed below 
   - GET: used to get the patient details using patient id.
   - POST: creates new patient record with updated filter data which is given above.
  - PUT: it updates the patient data, need to pass patient id.
  - DELETE: used to delete the patient record from FHIR server by passing patient id.


