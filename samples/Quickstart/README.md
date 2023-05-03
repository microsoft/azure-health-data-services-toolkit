<!-----
page_type: sample
languages:
- csharp
products:
- azure
- azure-healthcare-apis
description: Get started quickly with the Azure Health Data Services Toolkit on Azure Functions
----->
# Azure Health Data Services Toolkit Azure Function Custom Operation Quickstart

This quickstart will walk you through creating a simple custom operation on top of the FHIR Service using Azure Functions. We'll cover everything from deploying infrastructure, debugging locally, and deploying to Azure.

*This sample does not address authorization for simplicity - the endpoint is open to anyone with the address. Please only use test or sample data for this quickstart.*

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
- Login to the Azure CLI

## Setting up

This quickstart will create the below resources. These will be used both for local development and the Azure deployment.

- Azure Health Data Services workspace
- FHIR Service
- Function App (and associated storage account)
- APIM - [Azure API Management](https://learn.microsoft.com/azure/api-management/) (for Function App and Fhir Service)
- Log Analytics Workspace (for FHIR Service, Function App and APIM logs)
- Application Insights (for monitoring your custom operation)

### Deploy Quickstart

1. Create a new directory on your local machine and open that directory in a terminal or command prompt.
2. Setup your local Quickstart files .

    ```dotnetcli
    azd init --template Azure-Samples/azure-health-data-services-toolkit-fhir-function-quickstart
    ```
    > **Note:** lower-case name is needed to be compatible with all the resource types

3. If you want to use an existing FHIR Service, you need to open `infra/main.parameters.json` in a code editor and change the following settings:

    - `existingResourceGroupName`: The name of an existing resource group if you want to deploy your Function App there.
    - `existingAzureHealthDataServicesWorkspaceName`: The name of your existing Azure Health Data Services workspace.
    - `existingFhirServiceName`: The name of your existing FHIR Service.

4. By default, APIM is enabled for use. if you do not want to use APIM then pass `useAPIM` value as false Or  Open `infra/main.parameters.json` in a code editor and set the value of the parameter named `useAPIM` to false.
 

5. Next, you need to provision your Azure resources to run the sample with azd. If you are creating a new FHIR Service, this deploy may take 20 minutes.

    ```dotnetcli
    azd provision
    ```

6. To deploy your code (this can be done after local testing), run the deploy command.

    ```dotnetcli
    azd deploy
    ```

*Note*: For more information for developing on the Azure Health Data Services Toolkit, check out the [concepts document](https://github.com/microsoft/azure-health-data-services-toolkit/blob/main/docs/concepts.md).

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
4. For APIM endpoint, get APIM Gateway URL from section [Get the deployment details](##get-the-deployment-details) and test endpoint in API testing tool.

## Get the deployment details

 - Run the below command to get the deployed APIM Gateway URL variable named `APIM_GatewayUrl`.
    ```
     azd get-values
    ```

## Usage details
#### Quickstart function app

- `Program.cs` outlines how we can use Azure Function for Simple custom operation using various types of services like authenticator, headers and filters.
  - UseAuthenticator() Used for accessing Azure resources, it uses azure default credentials.
  - UseCustomHeaders()  Used  for custom headers Setup, using this service we can add custom header, here we have added custom header with name `X-MS-AZUREFHIR-AUDIT-USER-TOKEN-TEST`.
  - UseAzureFunctionPipeline() setup pipeline for Azure function.
  - AddInputFilter(typeof(QuickstartFilter)) Input filter added with name `QuickStart` which in turn used to modify the patient data using JsonTransform.
  - Add binding to pass the call to the FHIR service.
- Please refer `QuickstartFilter.cs` file for input filter modifications in the Patient Data.
  - Added language to resource as ‘en’ (English)
  - If there is no `Patient.meta.security` label, added [HTEST](https://www.hl7.org/fhir/resource-definitions.html#Meta.security)
- Custom operation QuickstartSample end point methods listed below.
  - GET: used to get the patient details using patient id.
  - POST: creates new patient record with updated filter data which is given above,to verify the new created record use GET method and pass created id.
  - PUT: it updates the patient data, need to pass patient id,to verify the updated record use GET method and pass updated id.
  - DELETE: used to delete the patient record from FHIR server by passing patient id, to verify the deleted record use GET method and pass deleted id.


#### APIM- Azure API Management

- APIM supports the complete API lifecycle, this template is prepared to use APIM for Fhir Service and Function App endpoints.
- in given APIM all the operations related to Patient are routed to QuickStart function app and for Fhir Service endpoints we have four methods like GET, POST, PUT, DELETE.

 
