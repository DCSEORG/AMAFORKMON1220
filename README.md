![Header image](https://github.com/DougChisholm/App-Mod-Assist/blob/main/repo-header.png)

# App-Mod-Assist
A modernized expense management system demonstrating how to transform legacy applications into cloud-native Azure solutions using Infrastructure as Code, managed identities, and AI-powered features.

## Overview

This project provides a complete, production-ready implementation of an expense management system with:
- **Modern Cloud Infrastructure**: Deployed to Azure using Bicep IaC
- **Zero Secrets**: Managed Identity authentication throughout
- **AI-Powered**: Optional GenAI chat interface for natural language queries
- **MCAPS Compliant**: Azure AD-only authentication for SQL Server
- **Enterprise Ready**: Swagger APIs, error handling, and comprehensive logging

## Quick Start

### Prerequisites
- Azure subscription
- Azure CLI installed and authenticated (`az login`)
- .NET 8 SDK (for local development)
- Python 3.x with pip (for database setup)

### Deployment Options

#### Option 1: Basic Deployment (App + Database)
```bash
# 1. Clone the repository
git clone <repo-url>
cd AMAFORKMON1220

# 2. Login to Azure
az login
az account set --subscription <subscription-id>

# 3. Deploy infrastructure and application
./deploy.sh
```

This deploys:
- App Service with managed identity
- Azure SQL Database with sample data
- Web application at `<app-url>/Index`

#### Option 2: Full Deployment (App + Database + AI)
```bash
# Deploy with GenAI features
./deploy-with-chat.sh
```

This deploys everything from Option 1 plus:
- Azure OpenAI with GPT-4o model
- AI-powered chat interface at `<app-url>/Chat`
- Natural language database queries

### What You Get

**Web Interface**
- 💰 Expense Management Dashboard (`/Index`)
- 💬 AI Chat Assistant (`/Chat`) - with full deployment only
- 📄 Interactive API Documentation (`/api-docs`)

**APIs**
- `GET /api/expenses` - Retrieve all expenses
- `GET /api/expenses/{id}` - Get specific expense
- `GET /api/users` - List all users
- `GET /api/categories` - List expense categories
- `POST /api/chat` - Send message to AI assistant

## Architecture

```
┌─────────────────────────────────────────┐
│         Azure Resource Group            │
│                                         │
│  ┌─────────────────┐                   │
│  │ Managed Identity│                   │
│  └────────┬─────────┘                   │
│           │                             │
│           ▼                             │
│  ┌─────────────────┐  ┌──────────────┐ │
│  │  App Service    │─▶│  Azure SQL   │ │
│  │  (.NET 8)       │  │  (Northwind) │ │
│  └────────┬─────────┘  └──────────────┘ │
│           │                             │
│           ▼                             │
│  ┌─────────────────┐                   │
│  │  Azure OpenAI   │                   │
│  │  (GPT-4o)       │                   │
│  └─────────────────┘                   │
└─────────────────────────────────────────┘
```

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed architecture documentation.

## Key Features

### 🔐 Security First
- **Managed Identity**: No credentials in code or configuration
- **Azure AD Authentication**: SQL Server with Azure AD-only mode (MCAPS compliant)
- **HTTPS Only**: TLS 1.2 minimum
- **Role-Based Access**: Proper database role assignments

### 🤖 AI-Powered
- Natural language queries: "Show me all pending expenses"
- Function calling: AI can invoke APIs to fetch real data
- Context-aware responses: Understands expense management domain

### 🏗️ Enterprise Grade
- **Infrastructure as Code**: All resources defined in Bicep
- **Automated Deployment**: One-command deployment scripts
- **Error Handling**: Graceful fallbacks with detailed error messages
- **API Documentation**: Interactive Swagger UI
- **Logging**: Comprehensive application logging

### 📊 Sample Data
- Pre-populated expense database
- Multiple users with different roles (Employee, Manager)
- Various expense categories
- Different approval statuses

## Local Development

For local development and testing, see [LOCAL_DEVELOPMENT.md](LOCAL_DEVELOPMENT.md).

Quick start for local development:
```bash
# 1. Install dependencies
cd app
dotnet restore

# 2. Login to Azure
az login

# 3. Update appsettings.json with your SQL Server FQDN

# 4. Run the application
dotnet run
```

Access at: https://localhost:5001/Index

## Project Structure

```
├── infrastructure/          # Bicep Infrastructure as Code
│   ├── main.bicep          # Main orchestration template
│   ├── app-service.bicep   # App Service and Managed Identity
│   ├── azure-sql.bicep     # SQL Server and Database
│   └── genai.bicep         # Azure OpenAI resources
├── app/                    # ASP.NET Core application
│   ├── Controllers/        # API controllers
│   ├── Models/            # Data models
│   ├── Pages/             # Razor Pages (UI)
│   └── Services/          # Business logic
├── Database-Schema/        # SQL schema files
├── deploy.sh              # Basic deployment script
├── deploy-with-chat.sh    # Full deployment with AI
├── run-sql.py             # Database schema import
└── run-sql-dbrole.py      # Database role configuration
```

## Security & Compliance

### MCAPS Compliance
- ✅ Azure AD-only authentication for SQL Server
- ✅ No SQL authentication enabled
- ✅ Managed Identity for service-to-service auth
- ✅ No secrets in code or configuration files

### Security Best Practices
- ✅ TLS 1.2 minimum
- ✅ HTTPS only
- ✅ Firewall rules for database access
- ✅ Role-based access control (RBAC)
- ✅ Latest .NET 8 LTS
- ✅ Updated security packages (no vulnerabilities)

## Troubleshooting

### Deployment Fails
- Ensure you're logged in: `az account show`
- Check you have proper permissions in the subscription
- Verify resource names are available in the region

### Database Connection Issues
- Verify your IP is in the firewall rules
- Check Managed Identity has db_datareader/db_datawriter roles
- Ensure Azure services can access SQL Server

### GenAI Not Working
- Verify you deployed with `deploy-with-chat.sh`
- Check App Service settings for OpenAI__Endpoint
- Ensure Managed Identity has Cognitive Services OpenAI User role

## Cost Considerations

**Basic Deployment** (deploy.sh):
- App Service (S1): ~$70/month
- SQL Database (Basic): ~$5/month
- **Total**: ~$75/month

**Full Deployment** (deploy-with-chat.sh):
- App Service (S1): ~$70/month
- SQL Database (Basic): ~$5/month
- Azure OpenAI (S0): Pay-per-use, varies by usage
- **Estimated Total**: ~$80-150/month depending on AI usage

💡 **Tip**: Delete resources when not in use to avoid charges
```bash
az group delete --name rg-appmodassist --yes --no-wait
```

## Contributing

This is a demonstration project. For real-world use:
1. Adjust SKUs for your production needs
2. Implement proper authentication/authorization
3. Add comprehensive unit and integration tests
4. Set up CI/CD pipelines
5. Configure monitoring and alerting
6. Review and adjust security policies

## Support

For issues specific to this implementation, please open an issue in the repository.

For Azure service questions:
- [Azure Documentation](https://docs.microsoft.com/azure/)
- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [App Service Documentation](https://learn.microsoft.com/azure/app-service/)

## License

See [LICENSE](LICENSE) file for details.
