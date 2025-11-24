# Local Development Instructions

This guide explains how to run and develop the application locally.

## Prerequisites

- .NET 8 SDK
- Azure CLI
- Azure subscription with access to Azure SQL Database
- Python 3.x (for database setup scripts)

## Setup Steps

### 1. Install Dependencies

```bash
cd app
dotnet restore
```

### 2. Configure Azure Authentication

Login to Azure CLI with your Azure account:

```bash
az login
az account set --subscription <your-subscription-id>
```

### 3. Update Connection String

The application is pre-configured for local development with Azure AD Default authentication.

In `app/appsettings.json`, the connection string is set to:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=tcp:localhost,1433;Database=Northwind;Authentication=Active Directory Default;"
}
```

**For connecting to an Azure SQL Database**, update it to:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=tcp:<your-server>.database.windows.net,1433;Database=Northwind;Authentication=Active Directory Default;"
}
```

The `Authentication=Active Directory Default` will use your `az login` credentials automatically.

### 4. Run the Application

```bash
cd app
dotnet run
```

The application will be available at:
- Main app: https://localhost:5001/Index
- API docs: https://localhost:5001/api-docs
- Chat UI: https://localhost:5001/Chat

## Development Workflow

### Building the Application

```bash
cd app
dotnet build
```

### Publishing for Deployment

```bash
cd app
dotnet publish -c Release -o ../publish
cd ../publish
zip -r ../app.zip .
```

### Testing APIs

Visit https://localhost:5001/api-docs to see Swagger documentation and test the APIs:
- `GET /api/expenses` - Get all expenses
- `GET /api/expenses/{id}` - Get expense by ID
- `GET /api/users` - Get all users
- `GET /api/categories` - Get all categories
- `POST /api/chat` - Send message to AI assistant

## Working with Azure SQL

### Authentication Modes

1. **Azure AD Default** (Local Development)
   - Uses your `az login` credentials
   - Best for local development
   - Configure: `Authentication=Active Directory Default`

2. **Azure AD Managed Identity** (Production)
   - Uses App Service's Managed Identity
   - Best for production deployments
   - Configure: `Authentication=Active Directory Managed Identity;User Id=<client-id>`

### Granting Database Access

To grant your Azure AD account access to the database:

```sql
CREATE USER [your-email@domain.com] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [your-email@domain.com];
ALTER ROLE db_datawriter ADD MEMBER [your-email@domain.com];
```

## GenAI Development

To develop with GenAI features locally:

1. Deploy Azure OpenAI using `deploy-with-chat.sh`
2. Update `appsettings.json` with the OpenAI endpoint:
```json
"OpenAI": {
  "Endpoint": "https://oai-appmodassist-xxxx.openai.azure.com/",
  "DeploymentName": "gpt-4o"
}
```
3. Run the app - it will use your Azure AD credentials to authenticate to OpenAI

## Troubleshooting

### Connection Errors

If you get authentication errors:
1. Verify you're logged in: `az account show`
2. Check your account has access to the SQL database
3. Ensure firewall rules allow your IP address

### Build Errors

If you get package restore errors:
```bash
dotnet nuget locals all --clear
dotnet restore
```

### Database Schema

To import the schema locally, use the Python scripts:
```bash
pip3 install pyodbc azure-identity
python3 run-sql.py
```

## Project Structure

```
app/
├── Controllers/        # API controllers
│   ├── ExpensesController.cs
│   ├── UsersController.cs
│   ├── CategoriesController.cs
│   └── ChatController.cs
├── Models/            # Data models
│   └── ExpenseModels.cs
├── Pages/             # Razor Pages
│   ├── Index.cshtml
│   ├── Index.cshtml.cs
│   ├── Chat.cshtml
│   └── Chat.cshtml.cs
├── Services/          # Business logic
│   ├── DatabaseService.cs
│   └── OpenAIService.cs
├── Program.cs         # Application entry point
└── appsettings.json   # Configuration
```
