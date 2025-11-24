targetScope = 'subscription'

@description('Resource group name')
param resourceGroupName string = 'rg-appmodassist'

@description('Location for all resources')
param location string = 'uksouth'

@description('Azure AD administrator Object ID')
param adminObjectId string

@description('Azure AD administrator login name')
param adminLogin string

@description('Deploy GenAI resources')
param deployGenAI bool = false

// Create resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
}

// Deploy App Service and Managed Identity
module appService 'app-service.bicep' = {
  scope: rg
  name: 'appServiceDeployment'
  params: {
    location: location
    baseName: 'appmodassist'
    uniqueSuffix: uniqueString(rg.id)
  }
}

// Deploy Azure SQL Database
module azureSQL 'azure-sql.bicep' = {
  scope: rg
  name: 'azureSQLDeployment'
  params: {
    location: location
    baseName: 'appmodassist'
    uniqueSuffix: uniqueString(rg.id)
    adminObjectId: adminObjectId
    adminLogin: adminLogin
    managedIdentityPrincipalId: appService.outputs.managedIdentityPrincipalId
  }
}

// Conditionally deploy GenAI resources
module genAI 'genai.bicep' = if (deployGenAI) {
  scope: rg
  name: 'genAIDeployment'
  params: {
    location: 'swedencentral'
    baseName: 'appmodassist'
    uniqueSuffix: uniqueString(rg.id)
    managedIdentityPrincipalId: appService.outputs.managedIdentityPrincipalId
  }
}

output resourceGroupName string = rg.name
output appServiceName string = appService.outputs.appServiceName
output appServiceUrl string = appService.outputs.appServiceUrl
output managedIdentityClientId string = appService.outputs.managedIdentityClientId
output managedIdentityName string = appService.outputs.managedIdentityName
output sqlServerName string = azureSQL.outputs.sqlServerName
output sqlServerFqdn string = azureSQL.outputs.sqlServerFqdn
output databaseName string = azureSQL.outputs.databaseName
output openAIEndpoint string = deployGenAI ? genAI.outputs.openAIEndpoint : ''
output openAIName string = deployGenAI ? genAI.outputs.openAIName : ''
output openAIModelName string = deployGenAI ? genAI.outputs.openAIModelName : ''
