# Architecture

## Overview

EcomShopping follows Clean Architecture principles with a layered approach that separates concerns and promotes maintainability, testability, and flexibility.

## Layer Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                         Presentation                         │
│  ┌──────────────────────┐      ┌──────────────────────┐    │
│  │   EcomShopping.Web   │      │   EcomShopping.API   │    │
│  │   (Blazor Server)    │      │   (REST API)         │    │
│  └──────────────────────┘      └──────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────────┐
│                         Application                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │          EcomShopping.Application                     │  │
│  │   • DTOs                                              │  │
│  │   • Interfaces                                        │  │
│  │   • Validators                                        │  │
│  │   • Business Logic                                    │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────────┐
│                         Domain                               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │          EcomShopping.Domain                          │  │
│  │   • Entities (Product, Order, Cart, etc.)             │  │
│  │   • Enums (OrderStatus, StockMovementType, etc.)      │  │
│  │   • Interfaces (IRepository, IProductRepository, etc.)│  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────────┐
│                      Infrastructure                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │        EcomShopping.Infrastructure                    │  │
│  │   • DbContext (EF Core)                               │  │
│  │   • Repositories                                      │  │
│  │   • External Services                                 │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────────┐
│                    Cross-Cutting Concerns                    │
│  ┌─────────────────┐  ┌────────────────┐  ┌────────────┐  │
│  │  Integration    │  │  File Import   │  │   Tests    │  │
│  │  • Abstractions │  │  • Core        │  │  • Unit    │  │
│  │  • Core         │  │  • Parsers     │  │  • Integ.  │  │
│  └─────────────────┘  └────────────────┘  └────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Layers

### Domain Layer (`EcomShopping.Domain`)

The core of the application containing:
- **Entities**: Business objects (Product, Category, Cart, Order, etc.)
- **Enums**: Value objects and status types
- **Interfaces**: Repository contracts

**Dependencies**: None (innermost layer)

### Application Layer (`EcomShopping.Application`)

Business logic and use cases:
- **DTOs**: Data transfer objects for API communication
- **Interfaces**: Service contracts
- **Validators**: FluentValidation rules
- **Services**: Business logic implementation

**Dependencies**: Domain layer

### Infrastructure Layer (`EcomShopping.Infrastructure`)

External concerns implementation:
- **Data Access**: Entity Framework Core DbContext
- **Repositories**: Concrete repository implementations
- **External Services**: Third-party integrations

**Dependencies**: Domain, Application layers

### Presentation Layer

#### API (`EcomShopping.API`)
- RESTful API controllers
- Swagger/OpenAPI documentation
- Dependency injection configuration
- Middleware (authentication, error handling)

#### Web (`EcomShopping.Web`)
- Blazor Server pages and components
- HTTP services for API communication
- UI state management

**Dependencies**: Application, Infrastructure layers

## Cross-Cutting Concerns

### Integration Engine
- **Abstractions**: Provider interfaces (IErpIntegration, ICrmIntegration, etc.)
- **Core**: Provider implementations and factory pattern

### File Import Engine
- **Core**: Import job tracking and orchestration
- **Parsers**: Excel, JSON, XML file parsers

## Data Flow

### Request Flow (API)
```
Client Request → Controller → Repository → Database
                     ↓
                 Validation
                     ↓
                Business Logic
```

### Response Flow (API)
```
Database → Repository → DTO Mapping → Controller → Client Response
```

### Page Request Flow (Blazor)
```
User Action → Component → HTTP Service → API → Repository → Database
```

## Design Patterns

### Repository Pattern
- Abstracts data access logic
- Provides clean separation between domain and data access
- Enables easy testing with mock repositories

### Dependency Injection
- All dependencies injected via constructor
- Configured in `Program.cs`
- Promotes loose coupling and testability

### Factory Pattern
- Integration provider factory
- File parser factory
- Creates instances based on configuration

### Options Pattern
- Configuration management
- Strongly-typed settings
- Environment-specific configurations

## Database Design

### Core Tables
- **Products**: Product catalog
- **Categories**: Hierarchical categories
- **Carts** / **CartItems**: Shopping cart
- **Orders** / **OrderItems**: Order management
- **Addresses**: Shipping and billing addresses
- **StockMovements**: Inventory tracking
- **ImportJobs**: File import history

### Relationships
- Product → Category (Many-to-One)
- Category → ParentCategory (Self-referencing)
- Cart → CartItems (One-to-Many)
- Product → CartItems (One-to-Many)
- Order → OrderItems (One-to-Many)
- Product → OrderItems (One-to-Many)
- Order → Addresses (Many-to-One for shipping/billing)
- Product → StockMovements (One-to-Many)

## Security Considerations

### Authentication & Authorization
- JWT token-based authentication (foundation in place)
- Role-based access control ready for implementation
- Admin endpoints protected by authorization attributes

### Data Protection
- SQL injection prevented via EF Core parameterized queries
- Input validation with FluentValidation
- CORS configured for cross-origin requests

## Scalability

### Horizontal Scaling
- Stateless API design
- Session state externalized (Redis ready)
- Container-ready with Docker

### Performance
- Async/await throughout
- Pagination for large datasets
- EF Core query optimization
- Caching strategy ready for implementation

## Testing Strategy

### Unit Tests
- Domain logic testing
- Repository mocking with Moq
- FluentAssertions for readable assertions

### Integration Tests
- End-to-end API testing
- Database integration tests
- Test database isolation

## Future Enhancements

- Event-driven architecture with message queues
- CQRS pattern for read/write separation
- API versioning
- Redis caching layer
- Real-time updates with SignalR
- Microservices decomposition
