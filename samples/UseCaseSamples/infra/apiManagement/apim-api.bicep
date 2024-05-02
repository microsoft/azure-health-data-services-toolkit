param name string

@description('Resource name to uniquely identify this API within the API Management service instance')
@minLength(1)
param apiName string

@description('The Display Name of the API')
@minLength(1)
@maxLength(300)
param apiDisplayName string

@description('Description of the API. May include HTML formatting tags.')
@minLength(1)
param apiDescription string

@description('Absolute URL of the backend function app for implementing this API.')
param functionAppUrl string

//@description('Function App Name')
// param functionAppName string
//@description('Default Key of the function app for implementing this API.')
//@secure()
//param functionAppKey string

@description('Absolute URL of the backend Fhir Service for implementing this API.')
param fhirServiceUrl string

param apimServiceLoggerId string

param useAPIM bool

var apiPolicyContent = loadTextContent('policies/apim-api-policy.xml')

var fhirApiPolicyContent = replace(loadTextContent('policies/apim-fhir-api-policy.xml'), '{fhirServiceUrl}', fhirServiceUrl)

var UseCasePolicyContent = replace(loadTextContent('policies/apim-usecase-api-policy.xml'), '{functionAppUrl}', functionAppUrl)


resource apimService 'Microsoft.ApiManagement/service@2021-08-01' existing = if(useAPIM) {
    name: name
}

resource quickStartApi 'Microsoft.ApiManagement/service/apis@2021-12-01-preview' = if(useAPIM) {
    name: apiName
    parent: apimService
    properties: {
        description: apiDescription
        displayName: apiDisplayName
        apiRevision: 'v1'
        subscriptionRequired: false
        serviceUrl: ''
        protocols: [
            'https'
        ]
        path: ''
    }

    resource apiPolicy 'policies' = {
        name: 'policy'
        properties: {
            format: 'rawxml'
            value: apiPolicyContent
        }
    }

    resource UseCaseSample 'operations' = {
        name: 'metadata'
        properties: {
            displayName: 'metadata'
            method: 'GET'
            urlTemplate: '/metadata'
        }

        resource QuickStartApiPolicy 'policies' = {
            name: 'policy'
            properties: {
                format: 'rawxml'
                value: UseCasePolicyContent
            }
        }
    }

    resource FHIRServerGet 'operations' = {
        name: 'get-fhir'
        properties: {
            displayName: 'FHIR Server'
            method: 'GET'
            urlTemplate: '/*'
        }

        resource fhirApiPolicy 'policies' = {
            name: 'policy'
            properties: {
                format: 'rawxml'
                value: fhirApiPolicyContent
            }
        }
    }

    resource FHIRServerPost 'operations' = {
        name: 'post-fhir'
        properties: {
            displayName: 'FHIR Server'
            method: 'POST'
            urlTemplate: '/*'
        }

        resource fhirApiPolicy 'policies' = {
            name: 'policy'
            properties: {
                format: 'rawxml'
                value: fhirApiPolicyContent
            }
        }
    }

    resource FHIRServerPut 'operations' = {
        name: 'put-fhir'
        properties: {
            displayName: 'FHIR Server'
            method: 'PUT'
            urlTemplate: '/*'
        }

        resource fhirApiPolicy 'policies' = {
            name: 'policy'
            properties: {
                format: 'rawxml'
                value: fhirApiPolicyContent
            }
        }
    }

    resource FHIRServerDelete 'operations' = {
        name: 'delete-fhir'
        properties: {
            displayName: 'FHIR Server'
            method: 'DELETE'
            urlTemplate: '/*'
        }

        resource fhirApiPolicy 'policies' = {
            name: 'policy'
            properties: {
                format: 'rawxml'
                value: fhirApiPolicyContent
            }
        }
    }

    resource smartApiDiagnostics 'diagnostics' = {
        name: 'applicationinsights'
        properties: {
            alwaysLog: 'allErrors'
            httpCorrelationProtocol: 'W3C'
            verbosity: 'information'
            logClientIp: true
            loggerId: apimServiceLoggerId
            sampling: {
                samplingType: 'fixed'
                percentage: 100
            }
        }
    }
}
