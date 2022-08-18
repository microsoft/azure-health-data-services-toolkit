# Developer Workflow

Creating custom operations with the Azure Health Data Services SDK requires developing against resources that exist in Azure only. This document will cover the recommended developer workflow to easily code *and* test custom operations locally before deploying to Azure.

We're only covering the recommended developer workflow for custom operations build and hosted on *Azure Function*.

## Core Concepts

*The [SDK concept document](./concepts.md) covers what each component does. If you are unfamiliar with terms like "custom operation" or "filter", check it out for context.*

- **Environment Setup**: Before working on your custom operation, you will need to setup your local and cloud environment.
- **Local Development**: 

## Local Development (Inner Loop)

Local development has two major requirements:
- The setup of the local tooling
- The deployment of cloud resources needed for testing

## Local Developer Setup



- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) 
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Azure Developer CLI](https://docs.microsoft.com/azure/developer/azure-developer-cli/get-started?tabs=bare-metal%2Cwindows&pivots=programming-language-csharp#prerequisites)
  - Quick install command on Windows: `powershell -ex AllSigned -c "Invoke-RestMethod 'https://aka.ms/install-azd.ps1' | Invoke-Expression"`
  - Quick install command on Linux/MacOS: `curl -fsSL https://aka.ms/install-azd.sh | bash`
- Visual Studio or Visual Studio Code
  - Or another code editor and proficiency with the command line

## Azure Setup
*Coming soon!*
- Azure subscription with permissions to create resource groups, resources, and create role assignments.

## General Developer Flow

*Coming soon!*


- Inner loop: Iterative single developer workflow of writing, building, and testing code.
= Outer loop(s): check-in to release


*If you have a deployment target other than Azure Functions, please file an issue and we'll investigate adding that developer flow here!*