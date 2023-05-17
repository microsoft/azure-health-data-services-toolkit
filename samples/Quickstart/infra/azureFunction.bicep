param storageAccountName string
param appServiceName string
param functionAppName string
param appInsightsInstrumentationKey string
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
    tags: appTags
}

@description('App Service used to run Azure Function')
resource hostingPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: appServiceName
  location: location
  kind: 'functionapp'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
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
            linuxFxVersion: 'dotnet-isolated|6.0'
            use32BitWorkerProcess: false
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

resource functionAppSettings 'Microsoft.Web/sites/config@2020-12-01' = {
    name: 'appsettings'
    parent: functionApp
    properties: union({
            AzureWebJobsStorage: 'DefaultEndpointsProtocol=https;AccountName=${funcStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${funcStorageAccount.listKeys().keys[0].value}'
            FUNCTIONS_EXTENSION_VERSION: '~4'
            FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
            APPINSIGHTS_INSTRUMENTATIONKEY: appInsightsInstrumentationKey
            APPLICATIONINSIGHTS_CONNECTION_STRING: 'InstrumentationKey=${appInsightsInstrumentationKey}'
            SCM_DO_BUILD_DURING_DEPLOYMENT: 'false'
            ENABLE_ORYX_BUILD: 'true'
        }, functionSettings)
}

var defaultHostKey = listkeys('${functionApp.id}/host/default', '2016-08-01').functionKeys.default
output functionAppKey string = defaultHostKey
output functionAppName string = functionAppName
output functionAppPrincipalId string = functionApp.identity.principalId
output hostName string = functionApp.properties.defaultHostName
output functionBaseUrl string = 'https://${functionApp.properties.defaultHostName}'
