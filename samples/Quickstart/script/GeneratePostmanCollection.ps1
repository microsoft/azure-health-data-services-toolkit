param (
 
	#Function App Resource Id
	[Parameter(Mandatory=$true)]
	[string]$functionAppResourceId
)

function parsefunctionAppResourceId{
	param (
	   [string]$resourceID
   )
   $resourceIdValuesArray = $resourceID.Split('/')
   $resourceGroup = 0..($resourceIdValuesArray.Length -1) | Where-Object {$resourceIdValuesArray[$_] -eq 'resourceGroups'}
   $functionApp = 0..($resourceIdValuesArray.Length -1) | Where-Object {$resourceIdValuesArray[$_] -eq 'sites'}
 
   #Extract Resource Group name
   $resourceGroupOutput = ''
   if(!$resourceGroup -eq ''){
	 $resourceGroupOutput = $resourceIdValuesArray.get($resourceGroup+1)
   }
   else{
	Write-Host -ForegroundColor Red "Invalid Function App Resource Id: Resource Group is missing"
	Exit
   }
 
   #Extract Function App name
   $functionAppOutput = ''
   if(!$functionApp -eq ''){
	 $functionAppOutput = $resourceIdValuesArray.get($functionApp+1)
   }
   else{
	Write-Host -ForegroundColor Red "Invalid Function App Resource Id: Function App is missing"
	Exit
   }
 
   $result = $resourceGroupOutput,$functionAppOutput
   return $result
}

try {
	$parsefunctionAppResourceIdOutput = parsefunctionAppResourceId -resourceID $functionAppResourceId
	
	#Resource Group
	$ResourceGroup = $parsefunctionAppResourceIdOutput[0]
 
	#Function App
	$FunctionApp = $parsefunctionAppResourceIdOutput[1]
	
	#Check Az Module and user logged in    
	Write-Host('Checking AZ module installed');
	$Azmodule_check = Get-Command az -ErrorVariable Azmodule_check -ErrorAction SilentlyContinue
	if (!$Azmodule_check) {
		Write-Host "Az CLI is not installed. Please install the az cli and re-run the script." -ForegroundColor Red
		Exit
	}
	
	#Log in and select subscription
	Write-Host('Log in and select the Subscription');
	az login

	Add-Type -AssemblyName System.Web

	#Fetch masterKey of the function app
	$masterKey = az functionapp keys list -g $ResourceGroup -n $FunctionApp --query masterKey -o tsv
	$code = [System.Web.HttpUtility]::UrlEncode($masterKey)
	
	#Create invoke URL for Patient function
	$patientFunctionInvokeURL = az functionapp function show --function-name Patient --name $FunctionApp --resource-group $ResourceGroup --query "invokeUrlTemplate" --output tsv
	$functionURL = "$patientFunctionInvokeURL" + "?code=$code"
	
	#Update case of 'p' in Patient endpoint
	$functionInvokeURL = $functionURL -replace '/patient/', '/Patient/'
	
	#Remove the '{id}' part to create invoke URL for POST method
	$functionUrlForPostMethod = $functionInvokeURL -replace "/\{id\}", ""
	
	#Extract the domain part from the functionInvokeURL
	$domainPart = ($functionInvokeURL -split '/')[2]

	#Split the domain part by '.' to get the components
	$hostSegments = $domainPart -split '\.'

	#Extract the path part from the URL
	$urlWithoutQuery = $functionInvokeURL.Split('?')[0]  # Remove the query string part
	$urlParts = $urlWithoutQuery.Split('/') | Where-Object { $_ -ne '' }

	#Extract path segments (skip domain part)
	$pathSegments = $urlParts[2..($urlParts.Length - 1)]

	#Repeate same steps for Post Function URL
	$domainPartOfPostURL = ($functionUrlForPostMethod -split '/')[2]
	
	$hostSegmentsOfPostURL = $domainPartOfPostURL -split '\.'
	
	$urlWithoutQueryforPostMethod = $functionUrlForPostMethod.Split('?')[0]  # Remove the query string part
	$urlPartsOfPostMethod = $urlWithoutQueryforPostMethod.Split('/') | Where-Object { $_ -ne '' }
	
	$pathSegmentsofPostMethod = $urlPartsOfPostMethod[2..($urlPartsOfPostMethod.Length - 1)]
	
	#Define the Postman collection
	$postmanCollection = @{
	info = @{
		_postman_id = [guid]::NewGuid().ToString()
		name = "Quickstart Collection"
		schema = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
	}
	item = @(
		 @{
			name = "GET"
			request = @{
				method = "GET"
				url = @{
					raw = $functionInvokeURL
					protocol = "https"
					host = $hostSegments
					path = $pathSegments
					query = @(
						@{
							key = "code"
							value = $code
						}
					)
				}
				header = @()
			}
			response = @()
		}
		 @{
			name = "POST"
			request = @{
				method = "POST"
				url = @{
					raw = $functionUrlForPostMethod 
					protocol = "https"
					host = $hostSegmentsOfPostURL
					path = $pathSegmentsofPostMethod
					query = @(
						@{
							key = "code"
							value = $code
						}
					)
				}
				header = @()
			}
			response = @()
		}
		 @{
			name = "PUT"
			request = @{
				method = "PUT"
				url = @{
					raw = $functionInvokeURL
					protocol = "https"
					host = $hostSegments
					path = $pathSegments
					query = @(
						@{
							key = "code"
							value = $code
						}
					)
				}
				header = @()
			}
			response = @()
		}
		 @{
			name = "DELETE"
			request = @{
				method = "DELETE"
				url = @{
					raw = $functionInvokeURL
					protocol = "https"
					host = $hostSegments
					path = $pathSegments
					query = @(
						@{
							key = "code"
							value = $code
						}
					)
				}
				header = @()
			}
			response = @()
		}
	)
}

	# Convert the Postman collection to JSON and save to a file
	$postmanCollectionJson = $postmanCollection | ConvertTo-Json -Depth 10
	$collectionPath = ".\PostmanCollection.json"
	Set-Content -Path $collectionPath -Value $postmanCollectionJson

	Write-Output "Postman collection saved to: $collectionPath"

	}
catch {
	# Error handling code - powershell catch exception message
	Write-Host -foregroundcolor Red "An error occurred: $_"
}
