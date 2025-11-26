# Infrastructure and Environment Setup

## Overview

This guide covers the infrastructure setup, environment configuration, and deployment strategies for the EcomShopping platform. It includes local development setup, CI/CD pipelines, containerization, and cloud deployment considerations.

## Table of Contents

1. [Local Development Environment](#local-development-environment)
2. [Docker and Containerization](#docker-and-containerization)
3. [CI/CD Pipeline](#cicd-pipeline)
4. [Environment Configuration](#environment-configuration)
5. [Cloud Deployment](#cloud-deployment)
6. [Monitoring and Logging](#monitoring-and-logging)
7. [Security and Secrets Management](#security-and-secrets-management)

## Local Development Environment

### Prerequisites

#### Required Software

1. **.NET 8.0 SDK**
   ```bash
   # Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   
   # Verify installation
   dotnet --version
   # Expected: 8.0.x or higher
   ```

2. **SQL Server**
   
   Choose one of the following:
   
   **Option A: SQL Server Express (Windows)**
   ```bash
   # Download from: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
   # Select "Express" edition
   ```
   
   **Option B: SQL Server LocalDB (Windows)**
   ```bash
   # Install with Visual Studio or standalone
   # Verify installation
   sqllocaldb info
   ```
   
   **Option C: SQL Server in Docker (Cross-platform)**
   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
     -p 1433:1433 --name sqlserver \
     -d mcr.microsoft.com/mssql/server:2022-latest
   ```

3. **Docker Desktop** (Optional for local containerized development)
   ```bash
   # Download from: https://www.docker.com/products/docker-desktop
   
   # Verify installation
   docker --version
   docker-compose --version
   ```

4. **Git**
   ```bash
   # Download from: https://git-scm.com/downloads
   
   # Verify installation
   git --version
   ```

#### Recommended Software

- **Visual Studio 2022** (Windows) or **Visual Studio Code** (Cross-platform)
- **Azure Data Studio** or **SQL Server Management Studio** (Database management)
- **Postman** or **REST Client** (API testing)

### Initial Setup

#### 1. Clone the Repository

```bash
git clone https://github.com/sorenbj/EcomShopping.git
cd EcomShopping
```

#### 2. Configure Connection Strings

**For API Project:**

Edit `src/EcomShopping.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EcomShoppingDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

**Connection String Options:**

- **SQL Server Express**: `Server=localhost\\SQLEXPRESS;Database=EcomShoppingDb;Trusted_Connection=True;`
- **SQL Server (Docker)**: `Server=localhost,1433;Database=EcomShoppingDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;`
- **Azure SQL**: `Server=tcp:{server}.database.windows.net,1433;Database=EcomShoppingDb;User Id={user};Password={password};Encrypt=True;`

#### 3. Install EF Core Tools

```bash
# Global tool installation
dotnet tool install --global dotnet-ef

# Verify installation
dotnet ef --version
# Expected: 8.0.x or higher
```

#### 4. Create and Update Database

```bash
# Navigate to Infrastructure project
cd src/EcomShopping.Infrastructure

# Create initial migration (if not exists)
dotnet ef migrations add InitialCreate --startup-project ../EcomShopping.API

# Apply migrations to database
dotnet ef database update --startup-project ../EcomShopping.API

# Return to solution root
cd ../..
```

#### 5. Build the Solution

```bash
# Restore dependencies
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test
```

#### 6. Run the Applications

**Run API:**
```bash
cd src/EcomShopping.API
dotnet run

# API will be available at:
# https://localhost:5147 (or port specified in launchSettings.json)
# Swagger UI: https://localhost:5147/swagger
```

**Run Web UI (in separate terminal):**
```bash
cd src/EcomShopping.Web
dotnet run

# Web will be available at:
# https://localhost:5148 (or port specified in launchSettings.json)
```

## Docker and Containerization

### Docker Compose Setup

The project includes a complete Docker Compose configuration for running all services locally.

#### Services Overview

- **sqlserver**: SQL Server 2022 Express
- **api**: EcomShopping REST API
- **web**: EcomShopping Blazor Web UI

#### Using Docker Compose

**Start all services:**
```bash
# Build and start containers
docker-compose up --build

# Run in detached mode
docker-compose up -d --build
```

**Stop services:**
```bash
docker-compose down

# Stop and remove volumes (deletes database data)
docker-compose down -v
```

**View logs:**
```bash
# All services
docker-compose logs

# Specific service
docker-compose logs api

# Follow logs
docker-compose logs -f web
```

**Restart a specific service:**
```bash
docker-compose restart api
```

#### Service URLs (Docker)

- **API**: http://localhost:5000 (HTTP), https://localhost:5001 (HTTPS)
- **Web**: http://localhost:5002 (HTTP), https://localhost:5003 (HTTPS)
- **SQL Server**: localhost:1433

#### Database Access in Docker

```bash
# Connect to SQL Server container
docker exec -it ecomshopping-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd"

# Or use Azure Data Studio / SSMS
# Server: localhost,1433
# User: sa
# Password: YourStrong@Passw0rd
```

### Docker Best Practices

#### Multi-stage Builds

Both API and Web Dockerfiles use multi-stage builds:

- **Stage 1 (base)**: Runtime image
- **Stage 2 (build)**: SDK for building
- **Stage 3 (publish)**: Publish optimized build
- **Stage 4 (final)**: Minimal runtime image

Benefits:
- Smaller final images (runtime only)
- Faster builds with layer caching
- Separate build and runtime dependencies

#### Environment Variables

Override configuration in docker-compose.yml:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=EcomShoppingDb;...
  - Logging__LogLevel__Default=Warning
```

## CI/CD Pipeline

### GitHub Actions Workflow

The project uses GitHub Actions for continuous integration and deployment.

#### Workflow File: `.github/workflows/ci.yml`

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches

#### Pipeline Stages

1. **Build Job**
   - Setup .NET 8.0
   - Restore dependencies
   - Build solution (Release configuration)
   - Run tests
   - Publish API and Web projects
   - Upload build artifacts

2. **Docker Job** (only on `main` branch)
   - Build Docker images
   - Push to Docker Hub (if credentials configured)

#### Running CI Pipeline Locally

You can simulate the CI pipeline locally:

```bash
# Restore dependencies
dotnet restore

# Build in Release mode
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release

# Publish API
dotnet publish src/EcomShopping.API/EcomShopping.API.csproj \
  --configuration Release \
  --output ./publish/api

# Publish Web
dotnet publish src/EcomShopping.Web/EcomShopping.Web.csproj \
  --configuration Release \
  --output ./publish/web
```

#### Adding Docker Hub Integration

Set GitHub secrets:

1. Go to repository Settings → Secrets and variables → Actions
2. Add secrets:
   - `DOCKER_USERNAME`: Your Docker Hub username
   - `DOCKER_PASSWORD`: Your Docker Hub password/token

The workflow will automatically push images when merged to `main`.

### Deployment Strategies

#### Blue-Green Deployment

Maintain two identical environments:
- **Blue**: Current production
- **Green**: New version

Process:
1. Deploy to Green environment
2. Run smoke tests
3. Switch traffic to Green
4. Keep Blue as rollback option

#### Rolling Updates

For Kubernetes deployments:
```yaml
strategy:
  type: RollingUpdate
  rollingUpdate:
    maxSurge: 1
    maxUnavailable: 0
```

#### Canary Deployment

Gradually roll out to subset of users:
1. Deploy to 10% of instances
2. Monitor metrics
3. Increase to 50%
4. Full rollout if successful

## Environment Configuration

### Environment Types

1. **Development**: Local development with detailed logging
2. **Testing**: Automated testing environment
3. **Staging**: Production-like for final validation
4. **Production**: Live environment

### Configuration Management

#### appsettings.json Hierarchy

```
appsettings.json                    # Base configuration
appsettings.Development.json        # Development overrides
appsettings.Staging.json            # Staging overrides
appsettings.Production.json         # Production overrides
```

#### Environment-Specific Settings

**Development:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

**Production:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Azure Key Vault Reference"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

#### Using Environment Variables

Override any configuration:

```bash
# Linux/macOS
export ConnectionStrings__DefaultConnection="Server=..."

# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Server=..."

# Windows CMD
set ConnectionStrings__DefaultConnection=Server=...
```

In Docker:
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Server=...
  - Logging__LogLevel__Default=Information
```

## Cloud Deployment

### Azure App Service

#### Prerequisites
- Azure subscription
- Azure CLI installed

#### Deploy API to Azure App Service

```bash
# Login to Azure
az login

# Create resource group
az group create --name EcomShopping-RG --location eastus

# Create App Service plan
az appservice plan create --name EcomShopping-Plan \
  --resource-group EcomShopping-RG \
  --sku B1 --is-linux

# Create Web App for API
az webapp create --name ecomshopping-api \
  --resource-group EcomShopping-RG \
  --plan EcomShopping-Plan \
  --runtime "DOTNET|8.0"

# Deploy from local build
az webapp deployment source config-zip \
  --resource-group EcomShopping-RG \
  --name ecomshopping-api \
  --src ./publish/api.zip
```

#### Deploy Web UI to Azure App Service

```bash
# Create Web App for Blazor UI
az webapp create --name ecomshopping-web \
  --resource-group EcomShopping-RG \
  --plan EcomShopping-Plan \
  --runtime "DOTNET|8.0"

# Deploy
az webapp deployment source config-zip \
  --resource-group EcomShopping-RG \
  --name ecomshopping-web \
  --src ./publish/web.zip
```

### Azure SQL Database

```bash
# Create SQL Server
az sql server create --name ecomshopping-sql \
  --resource-group EcomShopping-RG \
  --location eastus \
  --admin-user sqladmin \
  --admin-password "YourStrong@Passw0rd"

# Configure firewall
az sql server firewall-rule create \
  --resource-group EcomShopping-RG \
  --server ecomshopping-sql \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Create database
az sql db create --name EcomShoppingDb \
  --resource-group EcomShopping-RG \
  --server ecomshopping-sql \
  --service-objective S0
```

### Azure Container Instances

Deploy using Docker images:

```bash
# Deploy API container
az container create --name ecomshopping-api \
  --resource-group EcomShopping-RG \
  --image yourregistry/ecomshopping-api:latest \
  --dns-name-label ecomshopping-api \
  --ports 80 443

# Deploy Web container
az container create --name ecomshopping-web \
  --resource-group EcomShopping-RG \
  --image yourregistry/ecomshopping-web:latest \
  --dns-name-label ecomshopping-web \
  --ports 80 443
```

### Kubernetes (AKS)

For advanced orchestration:

```bash
# Create AKS cluster
az aks create --name EcomShopping-Cluster \
  --resource-group EcomShopping-RG \
  --node-count 3 \
  --enable-managed-identity \
  --generate-ssh-keys

# Get credentials
az aks get-credentials --name EcomShopping-Cluster \
  --resource-group EcomShopping-RG

# Deploy application
kubectl apply -f kubernetes/
```

## Monitoring and Logging

### Application Insights

Add to services:

```csharp
builder.Services.AddApplicationInsightsTelemetry(
    builder.Configuration["ApplicationInsights:ConnectionString"]);
```

### Health Checks

Implement health check endpoints:

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddUrlGroup(new Uri("https://api.example.com/health"), "API");

app.MapHealthChecks("/health");
```

### Logging with Serilog (Planned)

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces)
    .CreateLogger();
```

## Security and Secrets Management

### Azure Key Vault

Store secrets in Azure Key Vault:

```bash
# Create Key Vault
az keyvault create --name EcomShopping-Vault \
  --resource-group EcomShopping-RG \
  --location eastus

# Add secrets
az keyvault secret set --vault-name EcomShopping-Vault \
  --name ConnectionStrings--DefaultConnection \
  --value "Server=..."
```

Configure in application:

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### User Secrets (Development)

For local development:

```bash
# Initialize user secrets
cd src/EcomShopping.API
dotnet user-secrets init

# Add secret
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..."

# List secrets
dotnet user-secrets list
```

## Infrastructure as Code

### Terraform (Future)

Example infrastructure definition:

```hcl
resource "azurerm_app_service_plan" "main" {
  name                = "ecomshopping-plan"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  kind                = "Linux"
  
  sku {
    tier = "Standard"
    size = "S1"
  }
}
```

### ARM Templates (Alternative)

Azure Resource Manager templates for declarative infrastructure.

## Troubleshooting

### Common Issues

**Issue: Database connection fails**
```bash
# Check SQL Server is running
docker ps | grep sqlserver

# Test connection
sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT @@VERSION"
```

**Issue: Port already in use**
```bash
# Find process using port
# Linux/macOS
lsof -i :5000

# Windows
netstat -ano | findstr :5000

# Kill process or change port in launchSettings.json
```

**Issue: Docker build fails**
```bash
# Clear Docker build cache
docker builder prune

# Rebuild without cache
docker-compose build --no-cache
```

## Best Practices

1. **Never commit secrets** to source control
2. **Use environment variables** for configuration
3. **Implement health checks** for all services
4. **Monitor application performance** continuously
5. **Automate deployments** through CI/CD
6. **Use infrastructure as code** for reproducibility
7. **Implement proper logging** at all levels
8. **Regular security updates** for dependencies
9. **Backup databases** before deployments
10. **Test in staging** before production

## Next Steps

1. Set up local development environment
2. Configure CI/CD pipeline
3. Deploy to staging environment
4. Implement monitoring and alerting
5. Set up automated backups
6. Configure auto-scaling policies
7. Implement disaster recovery plan

## Resources

- [ASP.NET Core Deployment](https://docs.microsoft.com/aspnet/core/host-and-deploy/)
- [Azure App Service](https://azure.microsoft.com/services/app-service/)
- [Docker Documentation](https://docs.docker.com/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [GitHub Actions](https://docs.github.com/actions)
