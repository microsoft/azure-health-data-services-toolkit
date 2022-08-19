# Developer Workflow

Creating custom operations with the Azure Health Data Services SDK requires developing against resources that exist in Azure only. This document will cover the recommended developer workflow to easily code *and* test custom operations locally before deploying to Azure. We've focused on a fast developer "inner loop" with this SDK. It's built to quickly write and test code locally.

We're only covering the recommended developer workflow for custom operations build and hosted on *Azure Functions*.

## Overview

1. **Create From Template**: Copy the "Quickstart" sample to a new location.
2. **Provision Infrastructure**: Run `azd provision` to create needed cloud infrastructure.
3. **Develop and Test Locally**: Add the logic for your custom operation. Write tests and use the debugger.
4. **Deploy and Test in Azure**: Run `azd deploy` to deploy your code to Azure.
5. **Release via CI/CD Process**: Check your code into a git repository and use a CI/CD process to release.

<br />

![Typical development flow](/docs/images/custom_operation_development_flow.png)

### Setup your system

Before you start creating custom operations, you need to install some tools on your system that our developer workflow will require. Go through the tools below and make sure you have them installed on your system.

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Azure Developer CLI](https://docs.microsoft.com/azure/developer/azure-developer-cli/get-started?tabs=bare-metal%2Cwindows&pivots=programming-language-csharp#prerequisites)
  - Quick install command on Windows: `powershell -ex AllSigned -c "Invoke-RestMethod 'https://aka.ms/install-azd.ps1' | Invoke-Expression"`
  - Quick install command on Linux/MacOS: `curl -fsSL https://aka.ms/install-azd.sh | bash`
- Visual Studio or Visual Studio Code
  - Or another code editor and proficiency with the command line
- If you are using Visual Studio:
  - Azure Functions Tools. To add Azure Function Tools, include the Azure development workload in your Visual Studio installation.
- If you are using Visual Studio Code or something else:
  - [Azure Function Core Tools 4](https://docs.microsoft.com/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash#install-the-azure-functions-core-tools)

### 1. Create a custom operation from a template

First, copy the `samples/Quickstart` folder from this repository to somewhere else on your system. This will be your starting point for your custom operation.

Rename the folder, the solution file (`.sln`), and the project file (`.csproj`) to something else.

The Quickstart depends on some infrastructure templates at the root of the repository. Create a folder named `base` inside of your new folder and copy everything from the `infra` folder into this folder. Next, open the `main.bicep` file in the `infra` folder in your working directory. Find all instances of `../../../` and replace it with `base/`.

### 2. Provision infrastructure

Next, open a terminal or command prompt and go to the directory you created for your custom operation. Using the Azure Developer CLI, we can setup the needed resources for local development with a single command.

```bash
azd provision
```

This will deploy a minimal set of resources to use for your custom operation. If you need any other Azure resources for your custom operations, you will have to add them to the bicep template. If you need any information (like URL or resource name) from your new resources inside your custom operation, you will need to:

- Make sure to add the needed information to your `main.bicep` file as an `output` with the prefix `AZURE_`. This will allow loading into your local environment.
- Make sure to add the configuration to your Azure Function with the prefix `AZURE_`.
- Run `azd provision` again.

### 3. Develop and test locally

Now that your local and cloud environment is setup, you can add your business logic to your custom operation. You can use the template as a starting point, but this step is up to you.

This SDK does have patterns and tooling to help make this process easier. Check out the [SDK concept document](/docs/concepts.md) to see what each component does. Also checkout the [samples](/samples/) for inspiration.

If you copied the Quickstart template, it is already setup to use the output from the Azure Developer CLI inside your application as configuration. You can debug your custom operation locally.

### 4. Deploy and Test in Azure

Once you are done developing and testing your custom operation locally, you can deploy it to Azure using the Azure Developer CLI.

```bash
azd deploy
```

This will deploy your local code to the Azure Function that was created during the provision step. This command will output the URL of your Function App that you can use to test.

### 5. Release via CI/CD Process

*Coming soon once a pipeline is added to the Quickstart template!*
