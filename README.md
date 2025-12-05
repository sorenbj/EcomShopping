# EcomShopping - E-commerce Platform

[![Build Status](https://github.com/sorenbj/EcomShopping/workflows/CI/CD%20Pipeline/badge.svg)](https://github.com/sorenbj/EcomShopping/actions)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

An enterprise-grade e-commerce platform built with ASP.NET Core 8.0 and Blazor Server, featuring Clean Architecture, modular integrations, and comprehensive testing.

## 📋 Table of Contents

- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Features](#features)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Quick Start](#quick-start)
  - [Detailed Setup](#detailed-setup)
- [Documentation](#documentation)
- [Development](#development)
- [Testing](#testing)
- [Deployment](#deployment)
- [Contributing](#contributing)

## 🏗️ Architecture

The solution follows **Clean Architecture** principles with clear separation of concerns:

- **Domain Layer**: Core business entities and interfaces (no dependencies)
- **Application Layer**: Business logic, DTOs, and validation (depends on Domain)
- **Infrastructure Layer**: Data access, external service implementations (depends on Domain & Application)
- **API Layer**: RESTful API with Swagger documentation (depends on all layers)
- **Web Layer**: Blazor Server frontend for customer and admin interfaces (depends on all layers)

**Why Blazor Server?**
- Faster initial development with C# throughout the stack
- Smaller client payload (no WebAssembly download)
- Built-in real-time updates via SignalR
- Better SEO with server-side rendering
- Easy migration path to Blazor WebAssembly if needed

See [Architecture Documentation](docs/Architecture.md) for detailed diagrams and design patterns.

## 🛠️ Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| **Backend Framework** | ASP.NET Core | 8.0 |
| **Frontend Framework** | Blazor Server | 8.0 |
| **ORM** | Entity Framework Core | 8.0 |
| **Database** | SQL Server | 2022 |
| **API Documentation** | Swagger/OpenAPI | 3.0 |
| **Validation** | FluentValidation | 12.1.0 |
| **Testing** | xUnit, Moq, FluentAssertions | Latest |
| **Containerization** | Docker, Docker Compose | Latest |
| **CI/CD** | GitHub Actions | Latest |

**File Processing:**
- EPPlus 7.7.3 for Excel files
- System.Text.Json for JSON files
- System.Xml for XML files

See [Technology Stack Decisions](docs/Technology-Stack.md) for detailed rationale and alternatives considered.

## 📦 Project Structure

```
EcomShopping/
├── src/
│   ├── EcomShopping.Domain/              # 🎯 Core business entities
│   │   ├── Entities/                     # Product, Order, Cart, etc.
│   │   ├── Enums/                        # OrderStatus, StockMovementType
│   │   └── Interfaces/                   # Repository contracts
│   ├── EcomShopping.Application/         # 💼 Business logic layer
│   │   ├── DTOs/                         # Data transfer objects
│   │   ├── Interfaces/                   # Service contracts
│   │   └── Validators/                   # FluentValidation rules
│   ├── EcomShopping.Infrastructure/      # 🔧 Data & external services
│   │   ├── Data/                         # DbContext, configurations
│   │   ├── Repositories/                 # Repository implementations
│   │   └── Migrations/                   # EF Core migrations
│   ├── EcomShopping.API/                 # 🌐 REST API
│   │   ├── Controllers/                  # API endpoints
│   │   ├── Middleware/                   # Custom middleware
│   │   └── Dockerfile                    # API container definition
│   ├── EcomShopping.Web/                 # 🖥️ Blazor Server UI
│   │   ├── Components/                   # Razor components
│   │   ├── Pages/                        # Blazor pages
│   │   └── Dockerfile                    # Web container definition
│   ├── EcomShopping.Integration.Abstractions/  # 🔌 Integration interfaces
│   │   └── Interfaces/                   # IErpIntegration, ICrmIntegration, etc.
│   ├── EcomShopping.Integration.Core/    # 🔌 Integration implementations
│   │   ├── Engine/                       # Integration orchestration
│   │   ├── Providers/                    # Mock provider implementations
│   │   └── Scheduler/                    # Scheduled integration tasks
│   └── EcomShopping.FileImport.Core/     # 📄 File import engine
│       ├── Parsers/                      # Excel, JSON, XML parsers
│       └── Jobs/                         # Import job tracking
├── tests/
│   ├── EcomShopping.UnitTests/           # 🧪 Unit tests
│   └── EcomShopping.IntegrationTests/    # 🧪 Integration tests
├── docs/                                 # 📚 Documentation
│   ├── Architecture.md                   # Architecture overview
│   ├── Technology-Stack.md               # Technology decisions
│   ├── Database-Schema.md                # Database design
│   ├── Infrastructure-Setup.md           # Environment setup
│   ├── CICD-Pipeline.md                  # CI/CD documentation
│   ├── Integration-Engine-Guide.md       # Integration system guide
│   ├── File-Import-Guide.md              # File import engine guide
│   └── API.md                            # API documentation
├── .github/
│   └── workflows/
│       └── ci.yml                        # GitHub Actions pipeline
├── docker-compose.yml                    # Multi-container orchestration
├── EcomShopping.sln                      # Solution file
└── README.md                             # This file
```

## ✨ Features

### 🛍️ Product Catalog
- ✅ Product management with rich metadata
- ✅ Hierarchical category structure with unlimited depth
- ✅ SKU-based inventory tracking
- ✅ Product image management
- ✅ Active/inactive product status

### 🛒 Shopping Cart
- ✅ Session-based cart for anonymous users
- ✅ Persistent cart for authenticated users
- ✅ Real-time price calculation
- ✅ Cart persistence across sessions
- ✅ Automatic cart merging on login

### 📦 Order Management
- ✅ Order creation and tracking
- ✅ Order status workflow (Pending → Processing → Shipped → Delivered)
- ✅ Shipping and billing address management
- ✅ Unique order number generation
- ✅ Order history and search

### 📊 Inventory Management
- ✅ Stock movement tracking with audit trail
- ✅ Inventory adjustments (Purchase, Sale, Adjustment, Return, Damage)
- ✅ Real-time stock calculations
- ✅ **Stock reservation system during checkout (prevents overselling)**
- ✅ **Available stock calculation (actual - reserved)**
- ✅ **Low-stock alerts with configurable thresholds**
- ✅ **ERP integration for inventory synchronization**
- ✅ Movement reference tracking
- ✅ Automatic expiration of stock reservations

### 🔌 Integration Engine
- ✅ Modular provider architecture
- ✅ **ERP Integration**: Order sync, inventory updates, product data
- ✅ **CRM Integration**: Customer data synchronization
- ✅ **Shipping Providers**: Rate calculation, booking, tracking
- ✅ **Payment Gateways**: Payment processing, refunds, status checks
- ✅ Provider registry and factory pattern
- ✅ Scheduled and event-driven integrations

### 📄 File Import System
- ✅ **Excel (.xlsx)** file import with EPPlus
- ✅ **JSON** file import with System.Text.Json
- ✅ **XML** file import with System.Xml.Linq
- ✅ Generic file import engine with field mapping
- ✅ Product, Category, and User importers
- ✅ File upload API endpoint
- ✅ Admin UI with upload wizard and data preview
- ✅ Import job tracking and status monitoring
- ✅ Validation and error handling
- ✅ Extensible architecture for new file types and tables
- ✅ Import job tracking and history
- ✅ Comprehensive error logging per row
- ✅ Asynchronous processing support

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

| Requirement | Version | Download |
|-------------|---------|----------|
| **.NET SDK** | 8.0 or later | [Download](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **SQL Server** | 2022 / Express / LocalDB | [Download](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) |
| **Docker Desktop** (optional) | Latest | [Download](https://www.docker.com/products/docker-desktop) |
| **Git** | Latest | [Download](https://git-scm.com/downloads) |

**Verify installations:**
```bash
dotnet --version     # Should show 8.0.x
docker --version     # If using Docker
git --version
```

### Quick Start

Get up and running in 5 minutes:

```bash
# 1. Clone the repository
git clone https://github.com/sorenbj/EcomShopping.git
cd EcomShopping

# 2. Start with Docker Compose (easiest option)
docker-compose up --build

# 3. Access the applications
# - API: http://localhost:5000 (Swagger: http://localhost:5000/swagger)
# - Web: http://localhost:5002
# - SQL Server: localhost:1433 (sa / YourStrong@Passw0rd)
```

### Detailed Setup

For local development without Docker:

#### 1. Clone and Navigate

```bash
git clone https://github.com/sorenbj/EcomShopping.git
cd EcomShopping
```

#### 2. Configure Database Connection

Edit `src/EcomShopping.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EcomShoppingDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Alternative connection strings:**

- **SQL Server Express**: `Server=localhost\\SQLEXPRESS;Database=EcomShoppingDb;Trusted_Connection=True;`
- **SQL Server Docker**: `Server=localhost,1433;Database=EcomShoppingDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;`
- **Azure SQL**: `Server=tcp:{server}.database.windows.net,1433;Database=EcomShoppingDb;User Id={user};Password={password};Encrypt=True;`

#### 3. Install EF Core Tools

```bash
# Install globally
dotnet tool install --global dotnet-ef

# Verify installation
dotnet ef --version  # Should show 8.0.x
```

#### 4. Create and Migrate Database

```bash
# Navigate to Infrastructure project
cd src/EcomShopping.Infrastructure

# Create database and apply migrations
dotnet ef database update --startup-project ../EcomShopping.API

# Return to solution root
cd ../..
```

#### 5. Build and Test

```bash
# Restore dependencies
dotnet restore

# Build entire solution
dotnet build

# Run all tests (should show 26 passing tests)
dotnet test
```

#### 6. Run Applications

**Terminal 1 - Run API:**
```bash
cd src/EcomShopping.API
dotnet run

# API available at:
# - HTTPS: https://localhost:5147
# - HTTP: http://localhost:5146
# - Swagger: https://localhost:5147/swagger
```

**Terminal 2 - Run Web UI:**
```bash
cd src/EcomShopping.Web
dotnet run

# Web available at:
# - HTTPS: https://localhost:5148
# - HTTP: http://localhost:5149
```

**Note:** Port numbers may vary. Check console output for exact URLs.

## 📚 Documentation

Comprehensive guides are available in the `/docs` directory:

| Document | Description |
|----------|-------------|
| [Architecture](docs/Architecture.md) | System architecture, layers, and design patterns |
| [Technology Stack](docs/Technology-Stack.md) | Technology choices and rationale |
| [Database Schema](docs/Database-Schema.md) | Database design and EF Core migrations |
| [Infrastructure Setup](docs/Infrastructure-Setup.md) | Environment setup and deployment |
| [Inventory Management](docs/Inventory-Management.md) | Stock reservations, low-stock alerts, and ERP sync |
| [CI/CD Pipeline](docs/CICD-Pipeline.md) | GitHub Actions workflow documentation |
| [Integration Engine](docs/Integration-Engine-Guide.md) | Integration system developer guide |
| [File Import Engine](docs/File-Import-Guide.md) | File import system guide with examples |
| [API Documentation](docs/API.md) | REST API endpoints and examples |

## 👨‍💻 Development

### Code Style

The project uses `.editorconfig` for consistent code formatting. Most IDEs will automatically apply these settings.

**Key Conventions:**
- 4 spaces for indentation
- UTF-8 encoding
- LF line endings
- Trailing whitespace trimmed

### Adding New Features

Follow Clean Architecture principles:

1. **Start with Domain**: Create entities in `EcomShopping.Domain`
2. **Add Application Logic**: Create DTOs and interfaces in `EcomShopping.Application`
3. **Implement Infrastructure**: Add repositories and services in `EcomShopping.Infrastructure`
4. **Create API Endpoints**: Add controllers in `EcomShopping.API`
5. **Build UI**: Create Blazor components in `EcomShopping.Web`
6. **Write Tests**: Add tests in `EcomShopping.UnitTests` and `EcomShopping.IntegrationTests`

### Database Migrations

When modifying entities:

```bash
# Navigate to Infrastructure project
cd src/EcomShopping.Infrastructure

# Add new migration
dotnet ef migrations add YourMigrationName --startup-project ../EcomShopping.API

# Review generated migration code
# Edit if necessary

# Apply migration
dotnet ef database update --startup-project ../EcomShopping.API
```

**Best Practices:**
- Use descriptive migration names (e.g., `AddProductImageGallery`)
- Review generated SQL before applying
- Test migrations on development database first
- Keep migrations small and focused

### Running Locally

**Development Workflow:**

```bash
# 1. Make code changes
# 2. Build to check for errors
dotnet build

# 3. Run tests
dotnet test

# 4. Run API (Terminal 1)
cd src/EcomShopping.API
dotnet watch run  # Auto-restarts on changes

# 5. Run Web (Terminal 2)
cd src/EcomShopping.Web
dotnet watch run  # Auto-restarts on changes
```

## 🧪 Testing

The solution includes comprehensive test coverage:

### Test Structure

- **Unit Tests** (`EcomShopping.UnitTests`): Business logic and domain validation
- **Integration Tests** (`EcomShopping.IntegrationTests`): Database and API endpoint tests

### Running Tests

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/EcomShopping.UnitTests

# Run integration tests only
dotnet test tests/EcomShopping.IntegrationTests

# Run with detailed output
dotnet test --verbosity normal

# Run with code coverage (requires coverlet)
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Test Statistics

- **Total Tests**: 67
- **Unit Tests**: 62
- **Integration Tests**: 5
- **Success Rate**: 100%

### Writing Tests

**Example Unit Test:**

```csharp
[Fact]
public void Product_SetPrice_ShouldUpdatePrice()
{
    // Arrange
    var product = new Product();
    
    // Act
    product.Price = 99.99M;
    
    // Assert
    product.Price.Should().Be(99.99M);
}
```

## 📦 Deployment

### Docker Deployment

**Build and Run:**
```bash
# Build images
docker-compose build

# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

**Production Deployment:**
```bash
# Build for production
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### Cloud Deployment

See [Infrastructure Setup](docs/Infrastructure-Setup.md) for detailed instructions on deploying to:

- **Azure App Service**: Web Apps for API and Blazor
- **Azure Container Instances**: Container-based deployment
- **Azure Kubernetes Service (AKS)**: Orchestrated containers
- **AWS**: ECS, EKS, or Elastic Beanstalk
- **Google Cloud**: Cloud Run, GKE

### CI/CD

GitHub Actions automatically:
- ✅ Builds the solution
- ✅ Runs all tests
- ✅ Creates deployment artifacts
- ✅ Builds Docker images
- ✅ (Optional) Pushes to container registry

See [CI/CD Pipeline Documentation](docs/CICD-Pipeline.md) for configuration details.

## 🔧 Configuration

### Environment Variables

Override configuration with environment variables:

```bash
# Connection string
export ConnectionStrings__DefaultConnection="Server=..."

# Logging level
export Logging__LogLevel__Default="Information"

# Integration settings
export IntegrationSettings__Providers__ErpProvider__ApiKey="your-key"
```

### Secrets Management

**Development:**
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
```

**Production:**
- Use Azure Key Vault
- Or AWS Secrets Manager
- Or environment variables in container orchestration

## 🌐 API Endpoints

Explore the full API documentation:

- **Swagger UI**: `https://localhost:5147/swagger` (when running locally)
- **API Documentation**: [docs/API.md](docs/API.md)

### Quick Reference

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/products` | GET | List products with pagination |
| `/api/products/{id}` | GET | Get product details |
| `/api/products` | POST | Create product (admin) |
| `/api/cart` | GET | Get current cart |
| `/api/cart/items` | POST | Add item to cart |
| `/api/checkout` | POST | Create order from cart |
| `/api/orders` | GET | List user orders |
| `/api/stock/adjust` | POST | Adjust product stock |
| `/api/inventory/low-stock-alerts` | GET | Get low-stock alerts |
| `/api/inventory/available-stock/{id}` | GET | Get available stock for product |
| `/api/inventory/erp-sync` | POST | Sync stock from ERP |
| `/api/integrations/providers` | GET | List integration providers |
| `/api/imports/upload` | POST | Upload import file |

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Make** your changes following the code style guidelines
4. **Write** or update tests for your changes
5. **Commit** your changes (`git commit -m 'Add amazing feature'`)
6. **Push** to the branch (`git push origin feature/amazing-feature`)
7. **Open** a Pull Request

### Contribution Guidelines

- Follow Clean Architecture principles
- Write tests for new features
- Update documentation as needed
- Follow existing code style (.editorconfig)
- Ensure all tests pass before submitting PR
- Keep commits focused and atomic

## 📄 License

This project is licensed under the MIT License. See LICENSE file for details.

## 💬 Support

For questions, issues, or feature requests:

- **Issues**: [GitHub Issues](https://github.com/sorenbj/EcomShopping/issues)
- **Discussions**: [GitHub Discussions](https://github.com/sorenbj/EcomShopping/discussions)
- **Documentation**: Check the `/docs` directory

## 🙏 Acknowledgments

Built with:
- [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [FluentValidation](https://fluentvalidation.net/)
- [xUnit](https://xunit.net/)
- [Docker](https://www.docker.com/)

## 🗺️ Roadmap

### Current Version (v1.0)
- ✅ Clean Architecture foundation
- ✅ Product catalog and categories
- ✅ Shopping cart and checkout
- ✅ Order management
- ✅ Inventory tracking
- ✅ Integration engine framework
- ✅ File import system

### Upcoming Features
- 🔄 JWT Authentication and Authorization
- 🔄 User registration and login
- 🔄 Admin dashboard (Blazor UI)
- 🔄 Customer portal (Blazor UI)
- 🔄 Payment gateway integration (Stripe, PayPal)
- 🔄 Shipping provider integration (FedEx, UPS)
- 🔄 Email notifications
- 🔄 Product search and filtering
- 🔄 Product reviews and ratings
- 🔄 Wishlist functionality

### Future Enhancements
- Redis caching layer
- SignalR real-time notifications
- Advanced reporting and analytics
- Multi-currency support
- Multi-language support
- Mobile app (Blazor Hybrid)
- GraphQL API
- Microservices architecture

---

**⭐ Star this repository** if you find it helpful!

**📢 Follow** for updates on new features and releases.
