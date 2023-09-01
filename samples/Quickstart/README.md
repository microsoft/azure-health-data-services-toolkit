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
- [.NET 7.0](https://dotnet.microsoft.com/download)
- [Azure Command-Line Interface (CLI)](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Azure Developer CLI](https://docs.microsoft.com/azure/developer/azure-developer-cli/get-started?tabs=bare-metal%2Cwindows&pivots=programming-language-csharp#prerequisites)
- Visual Studio or Visual Studio Code
  - For Visual Studio, you will need the Azure Functions Tools. To add Azure Function Tools, include the Azure development workload in your Visual Studio installation.
  - For Visual Studio Code, you will need to install the [Azure Function Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash#install-the-azure-functions-core-tools).

### Prerequisite check

- [ ] In a terminal or command window, run `dotnet --version` to check that the .NET SDK is version 7.0 or later.
- [ ] Run `az --version` and `azd version` to check that you have the appropriate Azure command-line tools installed.
- [ ] Run `func --version` to check that you have version 4.0.5240 or later of the Azure Function Core Tools installed.
- [ ] Login to the Azure CLI using `az login`
- [ ] Login to Azure Developer CLI using `azd auth login`

## Setting up

This quickstart will create the below resources. These will be used both for local development and the Azure deployment.

- Azure Health Data Services workspace
- FHIR Service
- Function App (and associated storage account)
- APIM - [Azure API Management](https://learn.microsoft.com/azure/api-management/) (for Function App and Fhir Service)
- Log Analytics Workspace (for FHIR Service, Function App and APIM logs)
- Application Insights (for monitoring your custom operation)

### Deploy Quickstart

1. Make sure you have this repository cloned and up to date on your local system. Open the `samples/Quickstart` directory from this repository on your local system.
2. If you want to use an existing FHIR Service, you need to open `infra/main.parameters.json` in a code editor and change the following settings:

    - `existingResourceGroupName`: The name of an existing resource group if you want to deploy your Function App there.
    - `existingAzureHealthDataServicesWorkspaceName`: The name of your existing Azure Health Data Services workspace.
    - `existingFhirServiceName`: The name of your existing FHIR Service.

3. By default, APIM is enabled for use. if you do not want to use APIM then pass `useAPIM` value as false Or  Open `infra/main.parameters.json` in a code editor and set the value of the parameter named `useAPIM` to false.
 

4. Next, you will need to deploy the infrastructure and code. Open a terminal/command prompt into the `samples/Quickstart` folder in the context of this repository. NOTE: This may take 10-20 minutes if you are deploying a new FHIR Service.

    ```dotnetcli
    azd up
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

To get the deployed APIM Gateway URL variable named `APIM_GatewayUrl` Run the below command: 
```
azd get-values
```
If you don't want to use the APIM and are planning to call the Azure function instead, please follow the below steps to get the function url and key.

 1. Run the below command to get the deployed Azure function URL variable named `Azure_FunctionURL`.
    ```
     azd get-values
    ```
2. Run the below command to get the default key from the deployed Azure function.  
    ```
     az functionapp keys list --name
                         --resource-group
    ```
    
   As output, it will return the default key and master key.

    Please click on [link](https://learn.microsoft.com/en-us/cli/azure/functionapp/keys?view=azure-cli-latest#az-functionapp-keys-list) to read more about the function key.


## Usage details
### Quickstart function app

- `Program.cs` outlines how we can use Azure Function for Simple custom operation using various types of services like authenticator, headers and filters.
  - UseCustomHeaders()  Used  for custom headers Setup, using this service we can add custom header, here we have added custom header with name `X-MS-AZUREFHIR-AUDIT-USER-TOKEN-TEST`.
  - UseAzureFunctionPipeline() setup pipeline for Azure function.
  - AddInputFilter(typeof(QuickstartFilter)) Input filter added with name `QuickStart` which in turn used to modify the patient data using JsonTransform.
  - Add binding to pass the call to the FHIR service.
  - UseAuthenticator() Configures the binding to use an Azure.Identity DefaultAzureCredential.
- Please refer `QuickstartFilter.cs` file for input filter modifications in the Patient Data.
  - Added language to resource as ‘en’ (English)
  - If there is no `Patient.meta.security` label, added [HTEST](https://www.hl7.org/fhir/resource-definitions.html#Meta.security)
- Custom operation QuickstartSample end point methods listed below.
  - GET: used to get the patient details using patient id.
  - POST: creates new patient record with updated filter data which is given above,to verify the new created record use GET method and pass created id.
  - PUT: it updates the patient data, need to pass patient id,to verify the updated record use GET method and pass updated id.
  - DELETE: used to delete the patient record from FHIR server by passing patient id, to verify the deleted record use GET method and pass deleted id.


### APIM- Azure API Management

- APIM supports the complete API lifecycle, this template is prepared to use APIM for Fhir Service and Function App endpoints.
- in given APIM all the operations related to Patient are routed to QuickStart function app and for Fhir Service endpoints we have four methods like GET, POST, PUT, DELETE.

### Calling the Azure Function 

Please follow the below instructions if you want to perform operations using the Azure function instead of APIM.

  For POST,

     url : <QuickStartFunctionURL>/Patient?Code=defaultkey
    
  For GET/PUT/DELETE,
  
     url : <QuickStartFunctionURL>/Patient/{id}?Code=defaultkey
     

  -  Please copy the quickstartfunction URL from the above [command](##get-the-deployment-details) and replace it with `QuickStartFunctionURL`.

  - Copy the default key value from the above [command](##get-the-deployment-details) and replace it with 'defaultkey'. Remove the `?Code='defaultkey'` from the URL if AuthorizationLevel is Anonymous as it does not require any authorization.


 
