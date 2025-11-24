# Azure Services Architecture

This diagram shows the Azure services created by this deployment and how they connect to each other.

```
┌─────────────────────────────────────────────────────────────────────┐
│                           Azure Cloud                               │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                      Resource Group                           │  │
│  │                    rg-appmodassist                           │  │
│  │                                                              │  │
│  │  ┌────────────────────┐                                     │  │
│  │  │  User Assigned     │                                     │  │
│  │  │  Managed Identity  │                                     │  │
│  │  │  mid-AppModAssist  │                                     │  │
│  │  └─────────┬──────────┘                                     │  │
│  │            │                                                 │  │
│  │            │ Assigned to                                    │  │
│  │            │                                                 │  │
│  │            ▼                                                 │  │
│  │  ┌────────────────────┐         ┌──────────────────────┐   │  │
│  │  │   App Service      │         │   Azure SQL Server   │   │  │
│  │  │  (Standard S1)     │────────>│  sql-appmodassist    │   │  │
│  │  │  app-appmodassist  │  Auth   │                      │   │  │
│  │  │                    │  via    │  Database:           │   │  │
│  │  │  - .NET 8 App      │  MI     │  - Northwind         │   │  │
│  │  │  - Razor Pages     │         │                      │   │  │
│  │  │  - Web APIs        │         │  Azure AD Only Auth  │   │  │
│  │  │  - Chat UI         │         │  (MCAPS Compliant)   │   │  │
│  │  └─────────┬──────────┘         └──────────────────────┘   │  │
│  │            │                                                 │  │
│  │            │ Connects to (when deployed with GenAI)         │  │
│  │            │                                                 │  │
│  │            ▼                                                 │  │
│  │  ┌────────────────────┐                                     │  │
│  │  │  Azure OpenAI      │                                     │  │
│  │  │  oai-appmodassist  │                                     │  │
│  │  │  (Sweden Central)  │                                     │  │
│  │  │                    │                                     │  │
│  │  │  Model: GPT-4o     │                                     │  │
│  │  │  Auth: MI          │                                     │  │
│  │  └────────────────────┘                                     │  │
│  │                                                              │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘

         ▲
         │
         │ HTTPS
         │
    ┌────┴────┐
    │  Users  │
    └─────────┘
```

## Components

### 1. **App Service (app-appmodassist)**
- **Purpose**: Hosts the ASP.NET Core 8 web application
- **SKU**: Standard S1 (no cold start)
- **Features**:
  - Expense Management UI
  - RESTful APIs for data operations
  - AI-powered Chat Assistant
  - Swagger API documentation

### 2. **Azure SQL Database (Northwind)**
- **Purpose**: Stores expense management data
- **SKU**: Basic (development tier)
- **Security**:
  - Azure AD-only authentication (MCAPS compliant)
  - Managed Identity for app access
  - Firewall rules for Azure services

### 3. **User Assigned Managed Identity**
- **Purpose**: Secure authentication without credentials
- **Grants Access To**:
  - Azure SQL Database (db_datareader, db_datawriter roles)
  - Azure OpenAI (Cognitive Services OpenAI User role)

### 4. **Azure OpenAI (Optional - with deploy-with-chat.sh)**
- **Purpose**: Powers the AI Chat Assistant
- **Model**: GPT-4o (2024-08-06)
- **Region**: Sweden Central
- **Authentication**: Managed Identity
- **Features**:
  - Natural language queries
  - Function calling for database operations
  - Expense analytics and insights

## Authentication Flow

1. **App Service → SQL Database**
   - App Service uses its assigned Managed Identity
   - Authenticates to SQL Server using Azure AD
   - No connection strings or passwords needed

2. **App Service → Azure OpenAI**
   - App Service uses its assigned Managed Identity
   - Authenticates to OpenAI using DefaultAzureCredential
   - No API keys needed

3. **Local Development**
   - Developer runs `az login`
   - App uses Azure AD Default authentication
   - Same code works locally and in Azure

## Deployment Options

### Basic Deployment (deploy.sh)
- App Service + SQL Database
- No GenAI features
- Chat UI shows informational message

### Full Deployment (deploy-with-chat.sh)
- App Service + SQL Database + Azure OpenAI
- Full AI-powered chat functionality
- Natural language database queries

## Security Features

- ✅ Azure AD-only authentication (MCAPS compliant)
- ✅ Managed Identity (no secrets in code)
- ✅ HTTPS only
- ✅ TLS 1.2 minimum
- ✅ Role-based access control
- ✅ Network firewall rules
