targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the the environment which is used to generate a short unique hash used in all resources.')
param name string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string = ''

@description('Name of your existing Azure Health Data Services workspace (leave blank to create a new one)')
param existingAzureHealthDataServicesWorkspaceName string = ''

@description('Name of your existing FHIR Service (leave blank to create a new one)')
param existingFhirServiceName string = ''

@description('Name of your existing resource group (leave blank to create a new one)')
param existingResourceGroupName string = ''

var envRandomString = toLower(uniqueString(subscription().id, name, existingResourceGroupName, location))
var resourcePrefix = '${substring(name, 0, 11)}-${substring(envRandomString, 0, 5)}'

var createResourceGroup = empty(existingResourceGroupName) ? true : false
var createWorkspace = empty(existingAzureHealthDataServicesWorkspaceName) ? true : false
var createFhirService = empty(existingFhirServiceName) ? true : false

var appTags = {
  'azd-env-name': name
  'app-id': 'azure-health-data-services-toolkit-fhir-function-quickstart'
}

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = if (createResourceGroup) {
  name: '${name}-rg'
  location: location
  tags: appTags
}

resource existingResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = if (!createResourceGroup) {
  name: existingResourceGroupName
}

module template 'core.bicep'= if (createResourceGroup) {
  name: 'main'
  scope: resourceGroup
  params: {
    prefixName: resourcePrefix
    createWorkspace: createWorkspace
    createFhirService: createFhirService
    workspaceName: '${replace(resourcePrefix, '-', '')}ahds'
    fhirServiceName: 'testdata'
    location: location
    appTags: appTags
    fhirContributorPrincipals: [principalId]
  }
}

module existingResourceGrouptemplate 'core.bicep'= if (!createResourceGroup) {
  name: 'mainExistingResourceGroup'
  scope: existingResourceGroup
  params: {
    prefixName: resourcePrefix
    createWorkspace: createWorkspace
    createFhirService: createFhirService
    workspaceName: existingAzureHealthDataServicesWorkspaceName
    fhirServiceName: existingFhirServiceName
    location: location
    appTags: appTags
    fhirContributorPrincipals: [principalId]
  }
}

// These map to user secrets for local execution of the program
output LOCATION string = location
output FhirServerUrl string = createResourceGroup ? template.outputs.FhirServiceUrl : existingResourceGrouptemplate.outputs.FhirServiceUrl
