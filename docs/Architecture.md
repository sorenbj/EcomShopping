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
- **Blazor Server**: Interactive web UI using server-side rendering
- Razor components for reusable UI elements
- HTTP services for API communication
- SignalR for real-time client-server communication
- State management with cascading parameters and services

**Why Blazor Server:**
1. **Development Velocity**: Faster initial development with C# throughout the stack
2. **Reduced Complexity**: No need to manage WebAssembly deployment and versioning
3. **Smaller Payload**: Client downloads minimal JavaScript, not entire .NET runtime
4. **Security**: Business logic and data access remain server-side
5. **Real-time by Default**: Built-in SignalR integration for seamless real-time updates
6. **SEO Friendly**: Server-side rendering improves search engine optimization

**Migration Path to Blazor WebAssembly:**
- Component library can be shared between Server and WebAssembly
- API-first design allows easy client switching
- Hybrid rendering mode available (some components Server, others WebAssembly)
- Migration is incremental and non-breaking

**When to Consider WebAssembly:**
- Offline-first requirements
- Reduced server load for global deployment
- Progressive Web App (PWA) capabilities
- Client-side performance critical

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

## Cross-Cutting Concerns - Detailed

### Logging and Monitoring

**Current Implementation:**
- Built-in ASP.NET Core logging
- Structured logging ready with ILogger interface
- Log levels: Trace, Debug, Information, Warning, Error, Critical

**Planned Enhancements:**
- **Serilog**: Structured logging with multiple sinks
- **Application Insights**: Cloud-based monitoring and analytics
- **Correlation IDs**: Track requests across services
- **Performance Metrics**: Custom business metrics and KPIs

**Example Logging Pattern:**
```csharp
_logger.LogInformation(
    "Order {OrderNumber} created for user {UserId} with total {TotalAmount:C}",
    order.OrderNumber, order.UserId, order.TotalAmount);
```

### Error Handling

**Strategy:**
- Global exception handling middleware
- Consistent error response format
- Client-friendly error messages
- Detailed logging for debugging

**API Error Response Format:**
```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": [
    "Product SKU already exists",
    "Price must be greater than zero"
  ]
}
```

### Validation

**FluentValidation Integration:**
- Separate validator classes for each DTO
- Reusable validation rules
- Async validation support
- Localization ready

**Example:**
```csharp
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SKU).NotEmpty().Matches("^[A-Z0-9-]+$");
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

### Caching Strategy (Planned)

**Redis Integration:**
- Distributed caching for multi-instance deployments
- Session storage for Blazor Server
- Output caching for frequently accessed data

**Caching Policies:**
- Product catalog: Cache for 1 hour
- Categories: Cache for 24 hours
- User cart: Session-based
- Order history: No cache (always fresh)

### Authentication & Authorization

**JWT Implementation (Planned):**
- Token-based authentication
- Refresh token mechanism
- Claims-based authorization
- Role-based access control

**Roles:**
- Customer: Browse and purchase
- Admin: Full product and order management
- Manager: Reports and inventory oversight
- Support: Order and customer assistance

### API Rate Limiting (Planned)

**Protection Against Abuse:**
- Per-user rate limits
- Per-IP rate limits
- Sliding window algorithm
- Response headers indicating limits

**Configuration:**
```json
{
  "RateLimiting": {
    "PermitLimit": 100,
    "Window": "1m",
    "QueueLimit": 10
  }
}
```

### Health Checks

**Endpoint Monitoring:**
- Database connectivity
- External service availability
- Disk space and memory
- Custom business health indicators

**Implementation:**
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddUrlGroup(new Uri("https://payment-provider.com/health"), "PaymentProvider");

app.MapHealthChecks("/health");
```

### Background Tasks

**Hosted Services:**
- Integration scheduler for automated sync
- Email notification processing
- Report generation
- Data cleanup and archival

**Example:**
```csharp
public class IntegrationSchedulerService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessScheduledIntegrations();
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

### Configuration Management

**Hierarchical Configuration:**
1. appsettings.json (base)
2. appsettings.{Environment}.json (environment-specific)
3. User secrets (development)
4. Environment variables
5. Azure Key Vault (production)

**Strongly-Typed Options:**
```csharp
public class IntegrationSettings
{
    public Dictionary<string, ProviderSettings> Providers { get; set; }
}

builder.Services.Configure<IntegrationSettings>(
    builder.Configuration.GetSection("IntegrationSettings"));
```

### Localization and Internationalization (Future)

**Multi-language Support:**
- Resource files for UI strings
- Culture-specific formatting
- Time zone handling
- Currency conversion

### API Versioning (Planned)

**URL-based Versioning:**
- `/api/v1/products`
- `/api/v2/products`

**Benefits:**
- Backward compatibility
- Gradual migration
- Clear API evolution

### CORS Configuration

**Current Setup:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:5148")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
```

**Production Considerations:**
- Restrict origins to known domains
- Limit allowed methods
- Configure credential policies

## Future Enhancements

### Architecture Evolution

- **Event-driven architecture**: Message queues (RabbitMQ, Azure Service Bus)
- **CQRS pattern**: Separate read and write models for optimization
- **API versioning**: Support multiple API versions simultaneously
- **Redis caching layer**: Distributed cache for performance
- **SignalR enhancements**: Real-time notifications beyond Blazor's built-in support
- **Microservices decomposition**: Split into independently deployable services

### Advanced Features

- **GraphQL API**: Alternative to REST for flexible querying
- **gRPC**: High-performance inter-service communication
- **Event Sourcing**: Capture all state changes as events
- **Service Mesh**: Istio or Linkerd for service-to-service communication
- **Feature Flags**: Toggle features without deployment
- **A/B Testing**: Experimentation framework

### DevOps Enhancements

- **Kubernetes**: Container orchestration for production
- **Terraform**: Infrastructure as Code
- **Automated Performance Testing**: Load testing in CI/CD
- **Chaos Engineering**: Resilience testing
- **Blue-Green Deployments**: Zero-downtime deployments
- **Canary Releases**: Gradual rollout to production
