param apiManagementServiceName string
param location string
param sku string
param skuCount int
param publisherName string
param publisherEmail string
param appInsightsConnectionString string
param useAPIM bool
param appInsightsName string
param appInsightsExternalId string = '/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroup().name}/providers/microsoft.insights/components/${appInsightsName}'

resource apim 'Microsoft.ApiManagement/service@2021-12-01-preview' = if (useAPIM) {
  name: apiManagementServiceName
  location: location
  sku: {
    name: sku
    capacity: skuCount
  }

  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    publisherEmail: publisherEmail
    publisherName: publisherName
  }
 
  resource apimLogger 'loggers' = {
    name:  appInsightsName 
    properties: {
      loggerType: 'applicationInsights'
      credentials: {
        connectionString: appInsightsConnectionString
      }
      isBuffered: true
      resourceId: appInsightsExternalId
    }
  }

  resource apimDiagnostics 'diagnostics' = {
    name: 'applicationinsights'
    properties: {
      alwaysLog: 'allErrors'
      httpCorrelationProtocol: 'W3C'
      logClientIp: true
      loggerId: apimLogger.id
      sampling: {
        samplingType: 'fixed'
        percentage: 100
      }
      frontend: {
        request: {
          dataMasking: {
            queryParams: [
              {
                value: '*'
                mode: 'Hide'
              }
            ]
          }
        }
      }
      backend: {
        request: {
          dataMasking: {
            queryParams: [
              {
                value: '*'
                mode: 'Hide'
              }
            ]
          }
        }
      }
    }
  }
}

output apimName string = useAPIM ? apim.name:''
output serviceLoggerId string = useAPIM ? apim::apimLogger.id:''

