# EcomShopping - E-commerce Platform

An enterprise-grade e-commerce platform built with ASP.NET Core 8.0 and Blazor, featuring a modular architecture with integration engine and file import capabilities.

## Architecture

The solution follows Clean Architecture principles with clear separation of concerns:

- **Domain Layer**: Core business entities and interfaces
- **Application Layer**: Business logic, DTOs, and validation
- **Infrastructure Layer**: Data access, external service implementations
- **API Layer**: RESTful API with Swagger documentation
- **Web Layer**: Blazor Server frontend for customer and admin interfaces

## Technology Stack

- **Backend**: ASP.NET Core 8.0, Entity Framework Core, SQL Server
- **Frontend**: Blazor Server (.NET 8.0)
- **File Parsing**: EPPlus (Excel), System.Text.Json (JSON), System.Xml (XML)
- **API Documentation**: Swagger/OpenAPI
- **Validation**: FluentValidation
- **Testing**: xUnit, Moq, FluentAssertions
- **Containerization**: Docker, Docker Compose

## Project Structure

```
/src
  /EcomShopping.Domain              # Domain entities and interfaces
  /EcomShopping.Application          # Business logic and DTOs
  /EcomShopping.Infrastructure       # Data access and external services
  /EcomShopping.API                  # REST API
  /EcomShopping.Web                  # Blazor Server UI
  /EcomShopping.Integration.Abstractions  # Integration interfaces
  /EcomShopping.Integration.Core     # Integration implementations
  /EcomShopping.FileImport.Core      # File import engine
/tests
  /EcomShopping.UnitTests           # Unit tests
  /EcomShopping.IntegrationTests    # Integration tests
/docs                                # Documentation
```

## Features

### Product Catalog
- Product management with categories
- Hierarchical category structure
- SKU-based inventory tracking
- Image management

### Shopping Cart
- Session-based cart for anonymous users
- Persistent cart for authenticated users
- Real-time price calculation

### Order Management
- Order creation and tracking
- Order status workflow (Pending → Processing → Shipped → Delivered)
- Shipping and billing address management

### Inventory Management
- Stock movement tracking
- Inventory adjustments
- Low stock alerts foundation

### Integration Engine
- Modular provider architecture
- ERP integration (order sync, inventory updates)
- CRM integration (customer data sync)
- Shipping provider integration (rates, booking, tracking)
- Payment provider integration (processing, refunds)

### File Import System
- Excel (.xlsx) file import
- JSON file import
- XML file import
- Import job tracking and error logging

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server (LocalDB or full instance)
- Docker (optional, for containerized deployment)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/sorenbj/EcomShopping.git
   cd EcomShopping
   ```

2. **Update connection string**
   
   Edit `src/EcomShopping.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Your-Connection-String-Here"
     }
   }
   ```

3. **Create database and run migrations**
   ```bash
   cd src/EcomShopping.Infrastructure
   dotnet ef database update --startup-project ../EcomShopping.API
   ```

4. **Build the solution**
   ```bash
   dotnet build
   ```

5. **Run the API**
   ```bash
   cd src/EcomShopping.API
   dotnet run
   ```
   
   The API will be available at `https://localhost:5001` (or the port specified in launchSettings.json)
   
   Access Swagger UI at `https://localhost:5001/swagger`

6. **Run the Blazor Web App**
   ```bash
   cd src/EcomShopping.Web
   dotnet run
   ```

### Using Docker

1. **Build and run with Docker Compose**
   ```bash
   docker-compose up --build
   ```

   This will start:
   - API on port 5000 (HTTP) and 5001 (HTTPS)
   - Blazor Web on port 5002 (HTTP) and 5003 (HTTPS)
   - SQL Server on port 1433

2. **Stop containers**
   ```bash
   docker-compose down
   ```

## API Endpoints

### Products
- `GET /api/products` - Get paginated products with filtering
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create product (admin)
- `PUT /api/products/{id}` - Update product (admin)
- `DELETE /api/products/{id}` - Delete product (admin)

### Categories
- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get category by ID
- `POST /api/categories` - Create category (admin)
- `PUT /api/categories/{id}` - Update category (admin)
- `DELETE /api/categories/{id}` - Delete category (admin)

### Cart
- `GET /api/cart` - Get current cart
- `POST /api/cart/items` - Add item to cart
- `PUT /api/cart/items/{id}` - Update cart item
- `DELETE /api/cart/items/{id}` - Remove cart item

### Orders
- `POST /api/checkout` - Create order from cart
- `GET /api/orders` - Get user orders
- `GET /api/orders/{id}` - Get order by ID
- `PUT /api/orders/{id}/status` - Update order status (admin)

### Inventory
- `GET /api/inventory/products/{id}` - Get product inventory
- `POST /api/inventory/adjust` - Adjust inventory (admin)

### Imports
- `POST /api/imports/upload` - Upload file for import
- `GET /api/imports` - Get import history
- `GET /api/imports/{id}` - Get import job details
- `POST /api/imports/{id}/execute` - Execute import job

## Testing

### Run unit tests
```bash
dotnet test tests/EcomShopping.UnitTests
```

### Run integration tests
```bash
dotnet test tests/EcomShopping.IntegrationTests
```

### Run all tests
```bash
dotnet test
```

## Development

### Code Style

The project uses `.editorconfig` for consistent code formatting. Most IDEs will automatically apply these settings.

### Adding New Features

1. Start with domain entities in `EcomShopping.Domain`
2. Add interfaces and DTOs in `EcomShopping.Application`
3. Implement repositories and services in `EcomShopping.Infrastructure`
4. Create API endpoints in `EcomShopping.API`
5. Build UI components in `EcomShopping.Web`
6. Write tests in `EcomShopping.UnitTests` and `EcomShopping.IntegrationTests`

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.

## Support

For questions and support, please open an issue in the GitHub repository.
