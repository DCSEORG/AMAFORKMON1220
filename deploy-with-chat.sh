#!/bin/bash
set -e

echo "========================================"
echo "App Modernization with GenAI Deployment"
echo "========================================"
echo ""

# Configuration
RESOURCE_GROUP_NAME="rg-appmodassist"
LOCATION="uksouth"
SUBSCRIPTION_ID=$(az account show --query id -o tsv)

# Get current user info for SQL Server admin
echo "Getting current user information..."
ADMIN_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv)
ADMIN_LOGIN=$(az ad signed-in-user show --query userPrincipalName -o tsv)

echo "Subscription: $SUBSCRIPTION_ID"
echo "Admin Object ID: $ADMIN_OBJECT_ID"
echo "Admin Login: $ADMIN_LOGIN"
echo ""

# Deploy infrastructure (with GenAI)
echo "Step 1: Deploying infrastructure with GenAI resources..."
DEPLOYMENT_OUTPUT=$(az deployment sub create \
    --location $LOCATION \
    --template-file infrastructure/main.bicep \
    --parameters resourceGroupName=$RESOURCE_GROUP_NAME \
                 location=$LOCATION \
                 adminObjectId=$ADMIN_OBJECT_ID \
                 adminLogin=$ADMIN_LOGIN \
                 deployGenAI=true \
    --query properties.outputs \
    -o json)

echo "Deployment completed!"
echo ""

# Extract deployment outputs
APP_SERVICE_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.appServiceName.value')
APP_SERVICE_URL=$(echo $DEPLOYMENT_OUTPUT | jq -r '.appServiceUrl.value')
SQL_SERVER_FQDN=$(echo $DEPLOYMENT_OUTPUT | jq -r '.sqlServerFqdn.value')
DATABASE_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.databaseName.value')
MANAGED_IDENTITY_CLIENT_ID=$(echo $DEPLOYMENT_OUTPUT | jq -r '.managedIdentityClientId.value')
MANAGED_IDENTITY_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.managedIdentityName.value')
OPENAI_ENDPOINT=$(echo $DEPLOYMENT_OUTPUT | jq -r '.openAIEndpoint.value')
OPENAI_MODEL_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.openAIModelName.value')

echo "Deployment Outputs:"
echo "  App Service: $APP_SERVICE_NAME"
echo "  App URL: $APP_SERVICE_URL"
echo "  SQL Server: $SQL_SERVER_FQDN"
echo "  Database: $DATABASE_NAME"
echo "  Managed Identity: $MANAGED_IDENTITY_NAME"
echo "  OpenAI Endpoint: $OPENAI_ENDPOINT"
echo "  OpenAI Model: $OPENAI_MODEL_NAME"
echo ""

# Step 2: Configure App Service settings (including GenAI)
echo "Step 2: Configuring App Service settings..."
az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP_NAME \
    --name $APP_SERVICE_NAME \
    --settings \
        "ConnectionStrings__DefaultConnection=Server=tcp:$SQL_SERVER_FQDN,1433;Database=$DATABASE_NAME;Authentication=Active Directory Managed Identity;User Id=$MANAGED_IDENTITY_CLIENT_ID;" \
        "OpenAI__Endpoint=$OPENAI_ENDPOINT" \
        "OpenAI__DeploymentName=$OPENAI_MODEL_NAME" \
    --output none

echo "App Service settings configured!"
echo ""

# Step 3: Wait for SQL Server to be fully ready
echo "Step 3: Waiting 30 seconds for SQL Server to be fully ready..."
sleep 30
echo ""

# Step 4: Add current machine's IP to SQL Server firewall
echo "Step 4: Adding your IP to SQL Server firewall..."
MY_IP=$(curl -s https://api.ipify.org)
SQL_SERVER_NAME=$(echo $SQL_SERVER_FQDN | cut -d'.' -f1)

az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP_NAME \
    --server $SQL_SERVER_NAME \
    --name "AllowMyIP" \
    --start-ip-address $MY_IP \
    --end-ip-address $MY_IP \
    --output none

echo "Firewall rule added for IP: $MY_IP"
echo ""

# Step 5: Install Python dependencies
echo "Step 5: Installing Python dependencies..."
pip3 install --quiet pyodbc azure-identity
echo "Python dependencies installed!"
echo ""

# Step 6: Update Python scripts with actual server/database values
echo "Step 6: Updating Python scripts with deployment values..."
sed -i.bak "s/SERVER = \"example.database.windows.net\"/SERVER = \"$SQL_SERVER_FQDN\"/g" run-sql.py && rm -f run-sql.py.bak
sed -i.bak "s/DATABASE = \"Northwind\"/DATABASE = \"$DATABASE_NAME\"/g" run-sql.py && rm -f run-sql.py.bak

sed -i.bak "s/SERVER = \"example.database.windows.net\"/SERVER = \"$SQL_SERVER_FQDN\"/g" run-sql-dbrole.py && rm -f run-sql-dbrole.py.bak
sed -i.bak "s/DATABASE = \"Northwind\"/DATABASE = \"$DATABASE_NAME\"/g" run-sql-dbrole.py && rm -f run-sql-dbrole.py.bak

sed -i.bak "s/MANAGED-IDENTITY-NAME/$MANAGED_IDENTITY_NAME/g" script.sql && rm -f script.sql.bak

echo "Python scripts updated!"
echo ""

# Step 7: Import database schema
echo "Step 7: Importing database schema..."
python3 run-sql.py
echo ""

# Step 8: Configure database roles for managed identity
echo "Step 8: Configuring database roles for managed identity..."
python3 run-sql-dbrole.py
echo ""

# Step 9: Deploy application code
echo "Step 9: Deploying application code..."
if [ -f "app.zip" ]; then
    az webapp deploy \
        --resource-group $RESOURCE_GROUP_NAME \
        --name $APP_SERVICE_NAME \
        --src-path ./app.zip \
        --type zip \
        --output none
    echo "Application deployed successfully!"
else
    echo "Warning: app.zip not found. Please build the application first."
fi
echo ""

echo "========================================"
echo "Deployment completed successfully!"
echo "========================================"
echo ""
echo "Your application is available at:"
echo "  $APP_SERVICE_URL/Index"
echo ""
echo "Chat UI is available with GenAI features at:"
echo "  $APP_SERVICE_URL/Chat"
echo ""
echo "Note: Navigate to /Index to view the app"
