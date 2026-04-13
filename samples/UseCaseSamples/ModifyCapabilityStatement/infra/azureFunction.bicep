param storageAccountName string
param appServiceName string
param functionAppName string
param appInsightsConnectionString string
param location string
param functionSettings object = {}
param appTags object = {}

@description('Azure Function required linked storage account')
resource funcStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
    name: storageAccountName
    location: location
    kind: 'StorageV2'
    sku: {
        name: 'Standard_LRS'
    }
    properties: {
        allowSharedKeyAccess: false
    }
    tags: appTags
}

@description('App Service used to run Azure Function')
resource hostingPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: appServiceName
  location: location
  kind: 'functionapp,linux'
  sku: {
    name: 'S1'
    tier: 'Standard'
  }
  properties: {
    reserved: true
  }
  tags: appTags
}

@description('Azure Function used to run toolkit compute')
resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
    name: functionAppName
    location: location
    kind: 'functionapp,linux'

    identity: {
        type: 'SystemAssigned'
    }

    properties: {
        httpsOnly: true
        enabled: true
        serverFarmId: hostingPlan.id
        reserved: true
        clientAffinityEnabled: false
        siteConfig: {
            //netFrameworkVersion: 'v8.0'
            linuxFxVersion: 'dotnet-isolated|8.0'
            use32BitWorkerProcess: false
            alwaysOn:true
        }
    }

    tags: union(appTags, {
            'azd-service-name': 'func'
        })

    resource ftpPublishingPolicy 'basicPublishingCredentialsPolicies' = {
        name: 'ftp'
        // Location is needed regardless of the warning.
        #disable-next-line BCP187
        location: location
        properties: {
            allow: false
        }
    }

    resource scmPublishingPolicy 'basicPublishingCredentialsPolicies' = {
        name: 'scm'
        // Location is needed regardless of the warning.
        #disable-next-line BCP187
        location: location
        properties: {
            allow: false
        }
    }
}

resource functionAppSettings 'Microsoft.Web/sites/config@2022-09-01' = {
    name: 'appsettings'
    parent: functionApp
    properties: union({
            AzureWebJobsStorage__accountname: storageAccountName
            FUNCTIONS_EXTENSION_VERSION: '~4'
            FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
            APPLICATIONINSIGHTS_CONNECTION_STRING: appInsightsConnectionString
            SCM_DO_BUILD_DURING_DEPLOYMENT: 'false'
            ENABLE_ORYX_BUILD: 'false'
        }, functionSettings)
}

resource roleAssignment1 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
    name: guid('roleStorageBlobDataOwner-${storageAccountName}')
    properties: {
      roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b')
      principalId: functionApp.identity.principalId
    }
}

resource roleAssignment2 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
    name: guid('roleStorageAccountContributor-${storageAccountName}')
    properties: {
      roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '17d1049b-9a84-46fb-8f53-869881c3d3ab')
      principalId: functionApp.identity.principalId
    }
}


resource roleAssignment3 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
    name: guid('roleStorageQueueDataContributor-${storageAccountName}')
    properties: {
      roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '974c5e8b-45b9-4653-ba55-5f855dd0fb88')
      principalId: functionApp.identity.principalId
    }
}

output functionAppPrincipalId string = functionApp.identity.principalId
output hostName string = functionApp.properties.defaultHostName
output functionBaseUrl string = 'https://${functionApp.properties.defaultHostName}'
