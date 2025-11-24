# Deployment Instructions

This guide provides step-by-step instructions for deploying the modernized expense management application to Azure.

## Prerequisites Checklist

Before starting deployment, ensure you have:

- [ ] Azure subscription with appropriate permissions
- [ ] Azure CLI installed (version 2.50.0 or later)
- [ ] Python 3.x installed
- [ ] `jq` command-line JSON processor installed
- [ ] Logged in to Azure CLI: `az login`
- [ ] Selected correct subscription: `az account set --subscription <id>`

## Deployment Process

### Step 1: Clone the Repository

```bash
git clone <repository-url>
cd AMAFORKMON1220
```

### Step 2: Verify Prerequisites

```bash
# Check Azure CLI version
az --version

# Check Python version
python3 --version

# Check jq installation
jq --version

# Verify Azure login
az account show
```

### Step 3: Review Configuration (Optional)

You can customize the deployment by editing the parameters in the deployment scripts:

**deploy.sh** or **deploy-with-chat.sh**:
- `RESOURCE_GROUP_NAME`: Name of the resource group (default: rg-appmodassist)
- `LOCATION`: Azure region (default: uksouth)

**infrastructure/main.bicep**:
- Resource SKUs (App Service, SQL Database, etc.)
- Resource naming conventions
- Network configurations

### Step 4: Choose Deployment Option

#### Option A: Basic Deployment (No GenAI)

This deploys the core application without AI features.

```bash
chmod +x deploy.sh
./deploy.sh
```

**What gets deployed:**
- Azure Resource Group
- App Service with Standard S1 SKU
- User Assigned Managed Identity
- Azure SQL Server with Azure AD-only authentication
- Northwind database with sample data
- Web application with expense management UI

**Estimated deployment time:** 5-8 minutes

#### Option B: Full Deployment (With GenAI)

This deploys everything including AI-powered chat functionality.

```bash
chmod +x deploy-with-chat.sh
./deploy-with-chat.sh
```

**What gets deployed:**
- Everything from Option A, plus:
- Azure OpenAI Service in Sweden Central
- GPT-4o model deployment
- AI Chat interface with function calling
- Managed Identity role assignments for OpenAI

**Estimated deployment time:** 10-15 minutes

### Step 5: Monitor Deployment

The deployment script will output progress messages. Watch for:

1. ✅ Infrastructure deployment completion
2. ✅ App Service settings configuration
3. ✅ SQL Server readiness (30-second wait)
4. ✅ Firewall rule creation
5. ✅ Python dependencies installation
6. ✅ Database schema import
7. ✅ Database role configuration
8. ✅ Application deployment

### Step 6: Access Your Application

After successful deployment, the script will output URLs:

```
Your application is available at:
  https://app-appmodassist-xxxxx.azurewebsites.net/Index

Chat UI is available with GenAI features at:
  https://app-appmodassist-xxxxx.azurewebsites.net/Chat
```

**Important:** Navigate to `/Index` endpoint, not the root URL.

## Deployment Script Details

### What deploy.sh Does

1. **Validates prerequisites**: Checks Azure login and retrieves user info
2. **Deploys Bicep templates**: Creates all Azure resources
3. **Configures App Service**: Sets connection strings and app settings
4. **Waits for services**: Ensures SQL Server is ready (30-second wait)
5. **Configures firewall**: Adds your IP to SQL Server firewall
6. **Installs Python packages**: pyodbc and azure-identity
7. **Updates scripts**: Replaces placeholders with actual values
8. **Imports schema**: Executes database_schema.sql
9. **Configures roles**: Grants Managed Identity database access
10. **Deploys app**: Uploads and deploys app.zip

### What deploy-with-chat.sh Does

Includes all steps from deploy.sh, plus:
- Deploys Azure OpenAI resources
- Configures OpenAI endpoints in App Service settings
- Sets up Managed Identity for OpenAI access

## Post-Deployment Verification

### 1. Check Resource Group

```bash
az group show --name rg-appmodassist
```

### 2. List Deployed Resources

```bash
az resource list --resource-group rg-appmodassist --output table
```

### 3. Verify App Service

```bash
az webapp show --name <app-service-name> --resource-group rg-appmodassist
```

### 4. Test the Application

Open your browser and navigate to:
- Main app: `https://<app-name>.azurewebsites.net/Index`
- API docs: `https://<app-name>.azurewebsites.net/api-docs`
- Chat UI: `https://<app-name>.azurewebsites.net/Chat`

### 5. Test APIs

Using the Swagger UI at `/api-docs`, test:
- GET /api/expenses - Should return sample expense data
- GET /api/users - Should return user list
- GET /api/categories - Should return expense categories

### 6. Test Chat (Full Deployment Only)

Navigate to `/Chat` and try queries like:
- "Show me all expenses"
- "What's the total amount of pending expenses?"
- "How many expenses do we have?"

## Troubleshooting

### Deployment Fails at Bicep Stage

**Issue**: Bicep template validation or deployment error

**Solutions**:
- Check Azure permissions: Need Contributor or Owner role
- Verify subscription limits: App Service and SQL Database quotas
- Check resource name availability: Names must be globally unique
- Review error message in deployment output

```bash
# View deployment status
az deployment sub show --name <deployment-name>

# View deployment operations
az deployment sub operation list --name <deployment-name>
```

### Database Schema Import Fails

**Issue**: run-sql.py exits with error

**Solutions**:
- Verify Azure CLI login: `az account show`
- Check firewall rules: Your IP must be allowed
- Wait longer: SQL Server might not be fully ready
- Check ODBC driver: `odbcinst -j` (Linux/Mac)

```bash
# Re-run schema import manually
python3 run-sql.py

# Check SQL Server firewall rules
az sql server firewall-rule list --server <server-name> --resource-group rg-appmodassist
```

### Application Deployment Fails

**Issue**: app.zip deployment error

**Solutions**:
- Verify app.zip exists in repository root
- Check App Service status: Should be Running
- Review App Service logs

```bash
# Check App Service logs
az webapp log tail --name <app-name> --resource-group rg-appmodassist

# Manually deploy app.zip
az webapp deploy --resource-group rg-appmodassist --name <app-name> --src-path ./app.zip --type zip
```

### Database Connection Errors

**Issue**: Application shows "Database Connection Error"

**Solutions**:
- Verify Managed Identity has database roles: run-sql-dbrole.py
- Check connection string in App Service settings
- Ensure SQL Server allows Azure services

```bash
# Check App Service settings
az webapp config appsettings list --name <app-name> --resource-group rg-appmodassist

# Re-run role configuration
python3 run-sql-dbrole.py
```

### OpenAI Not Working

**Issue**: Chat returns error or placeholder message

**Solutions**:
- Verify OpenAI deployment: Check in Azure Portal
- Check App Service settings for OpenAI__Endpoint
- Verify Managed Identity role assignment

```bash
# Check OpenAI deployments
az cognitiveservices account deployment list --name <openai-name> --resource-group rg-appmodassist

# Verify role assignment
az role assignment list --assignee <managed-identity-principal-id> --scope <openai-resource-id>
```

## Cleanup

To remove all deployed resources and avoid charges:

```bash
# Delete the entire resource group
az group delete --name rg-appmodassist --yes --no-wait

# Verify deletion
az group exists --name rg-appmodassist
```

**Warning**: This permanently deletes all resources including databases. Export any data you need before deletion.

## Re-deployment

To redeploy after cleanup or updates:

1. If you made code changes, rebuild app.zip:
   ```bash
   cd app
   dotnet publish -c Release -o ../publish
   cd ../publish
   zip -r ../app.zip .
   cd ..
   ```

2. Run the deployment script again:
   ```bash
   ./deploy.sh
   # or
   ./deploy-with-chat.sh
   ```

The script will recreate all resources and deploy the updated application.

## Advanced Configuration

### Using Different Azure Regions

Edit the deployment script:
```bash
LOCATION="westeurope"  # Change to your preferred region
```

**Note**: For GenAI features, OpenAI is deployed to Sweden Central regardless of app location, as GPT-4o is required to be in that region.

### Customizing Resource Names

Edit `infrastructure/main.bicep`:
```bicep
param baseName string = 'myapp'  // Change from 'appmodassist'
```

### Using Existing Database

If you want to connect to an existing database instead of creating a new one:

1. Edit `deploy.sh` to skip Bicep deployment
2. Manually configure App Service settings with your database connection string
3. Run schema import scripts against your database

## Security Considerations

### Secrets Management

This deployment uses Managed Identity exclusively - no secrets needed!

### Firewall Configuration

The deployment script adds your current public IP to SQL Server firewall. To add additional IPs:

```bash
az sql server firewall-rule create \
  --resource-group rg-appmodassist \
  --server <server-name> \
  --name "AdditionalIP" \
  --start-ip-address <ip> \
  --end-ip-address <ip>
```

### Production Recommendations

Before using in production:
1. Review and adjust App Service SKU for expected load
2. Enable App Service backup and disaster recovery
3. Configure Azure Monitor alerts
4. Set up Azure Application Insights
5. Implement WAF (Web Application Firewall)
6. Use Azure Private Link for SQL Database
7. Enable database backups and point-in-time restore
8. Implement CI/CD pipeline for deployments
9. Add comprehensive logging and monitoring

## Support

For deployment issues:
1. Check the troubleshooting section above
2. Review Azure Portal for resource status
3. Check App Service logs for runtime errors
4. Review GitHub repository issues

For Azure service issues:
- [Azure Support](https://azure.microsoft.com/support/)
- [Azure Documentation](https://docs.microsoft.com/azure/)
