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

var resourceToken = toLower(uniqueString(subscription().id, name, location))

var appTags = {
  'azd-env-name': name
  'app-id': 'azure-health-data-services-sdk'
  'sample-name': 'Quickstart'
}

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${name}-rg'
  location: location
  tags: appTags
}

module template 'core.bicep'= {
  name: 'main'
  scope: resourceGroup
  params: {
    prefixName: resourceToken
    workspaceName: '${resourceToken}ahds'
    fhirServiceName: 'testdata'
    location: location
    additionalTags: appTags
    fhirContributorPrincipals: [principalId]
  }
}

// These map to user secrets for local execution of the program
output LOCATION string = location
output FhirServerUrl string = template.outputs.FhirServiceUrl
