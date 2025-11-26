# Database Schema Planning and Migration Strategy

## Overview

This document outlines the database schema design, Entity Framework Core configuration, migration strategy, and data management approach for the EcomShopping platform.

## Database Design Philosophy

### Code-First Approach

**Rationale:**
- Version control for database schema changes
- Automatic migration generation from model changes
- Type-safe database operations through LINQ
- Easy to refactor and evolve schema
- Consistent across development, testing, and production

### Entity Framework Core Features

- **Migrations**: Incremental schema changes tracked in code
- **Fluent API**: Explicit configuration for relationships and constraints
- **Conventions**: Sensible defaults reduce configuration
- **Indexes**: Performance optimization through strategic indexing
- **Seeding**: Initial data and test data management

## Schema Overview

### Entity Relationship Diagram

```
┌─────────────────┐         ┌──────────────────┐
│   Categories    │◄────────┤    Products      │
│                 │ 1     * │                  │
│ - Id            │         │ - Id             │
│ - Name          │         │ - Name           │
│ - Description   │         │ - SKU            │
│ - ParentId      │─┐       │ - Price          │
└─────────────────┘ │       │ - CategoryId     │
        ▲           │       │ - ImageUrl       │
        └───────────┘       └──────────────────┘
     Self-Reference                 │
                                    │ 1
                   ┌────────────────┼────────────────┐
                   │                │                │
                   │ *              │ *              │ *
         ┌─────────┴────────┐  ┌───┴──────────┐  ┌──┴─────────────┐
         │   CartItems      │  │  OrderItems  │  │ StockMovements │
         │                  │  │              │  │                │
         │ - Id             │  │ - Id         │  │ - Id           │
         │ - CartId         │  │ - OrderId    │  │ - ProductId    │
         │ - ProductId      │  │ - ProductId  │  │ - Quantity     │
         │ - Quantity       │  │ - Quantity   │  │ - Type         │
         │ - UnitPrice      │  │ - UnitPrice  │  │ - Timestamp    │
         └──────────────────┘  └──────────────┘  └────────────────┘
                  │ *                    │ *
                  │ 1                    │ 1
         ┌────────┴────────┐    ┌────────┴──────────┐
         │     Carts       │    │      Orders       │
         │                 │    │                   │
         │ - Id            │    │ - Id              │
         │ - SessionId     │    │ - OrderNumber     │
         │ - UserId        │    │ - UserId          │
         │ - CreatedDate   │    │ - OrderDate       │
         └─────────────────┘    │ - Status          │
                                │ - TotalAmount     │
                                │ - ShippingAddrId  │
                                │ - BillingAddrId   │
                                └───────────────────┘
                                        │ *
                                        │ 1
                                ┌───────┴────────┐
                                │   Addresses    │
                                │                │
                                │ - Id           │
                                │ - Street       │
                                │ - City         │
                                │ - State        │
                                │ - PostalCode   │
                                │ - Country      │
                                └────────────────┘

┌─────────────────┐
│   ImportJobs    │
│                 │
│ - Id            │
│ - FileName      │
│ - FileType      │
│ - Status        │
│ - UploadedDate  │
│ - ProcessedDate │
│ - RowsProcessed │
│ - RowsFailed    │
│ - ErrorLog      │
└─────────────────┘
```

## Core Tables

### Products

Primary entity for the product catalog.

**Columns:**
- `Id` (int, PK): Primary key
- `Name` (nvarchar(200)): Product name
- `Description` (nvarchar(max)): Detailed description
- `SKU` (nvarchar(50), unique): Stock keeping unit
- `Price` (decimal(18,2)): Unit price
- `CategoryId` (int, FK): Reference to Categories
- `ImageUrl` (nvarchar(500)): Product image URL
- `IsActive` (bit): Whether product is available
- `CreatedDate` (datetime2): Record creation timestamp
- `ModifiedDate` (datetime2): Last modification timestamp

**Indexes:**
- Clustered index on `Id`
- Unique non-clustered index on `SKU`
- Non-clustered index on `CategoryId`
- Non-clustered index on `IsActive, Price` (for filtering)

**Constraints:**
- FK to Categories (CategoryId)
- Check constraint: Price >= 0

### Categories

Hierarchical product categorization.

**Columns:**
- `Id` (int, PK): Primary key
- `Name` (nvarchar(100)): Category name
- `Description` (nvarchar(500)): Category description
- `ParentCategoryId` (int, nullable, FK): Self-reference for hierarchy
- `IsActive` (bit): Whether category is active
- `DisplayOrder` (int): Sort order for display
- `CreatedDate` (datetime2): Record creation timestamp

**Indexes:**
- Clustered index on `Id`
- Non-clustered index on `ParentCategoryId`
- Non-clustered index on `IsActive, DisplayOrder`

**Constraints:**
- FK to Categories (ParentCategoryId) - self-referencing

**Sample Hierarchy:**
```
Electronics (Parent)
├── Computers (ParentId = Electronics.Id)
│   ├── Laptops (ParentId = Computers.Id)
│   └── Desktops (ParentId = Computers.Id)
└── Mobile Phones (ParentId = Electronics.Id)
```

### Carts

Shopping cart for both anonymous and authenticated users.

**Columns:**
- `Id` (int, PK): Primary key
- `SessionId` (nvarchar(100)): Browser session identifier
- `UserId` (nvarchar(450), nullable): Authenticated user ID
- `CreatedDate` (datetime2): When cart was created
- `ModifiedDate` (datetime2): Last cart modification

**Indexes:**
- Clustered index on `Id`
- Non-clustered index on `SessionId`
- Non-clustered index on `UserId`

**Business Logic:**
- Anonymous users: SessionId-based cart
- Authenticated users: UserId-based cart
- Merge cart on login if both exist

### CartItems

Items within a shopping cart.

**Columns:**
- `Id` (int, PK): Primary key
- `CartId` (int, FK): Reference to Carts
- `ProductId` (int, FK): Reference to Products
- `Quantity` (int): Number of items
- `UnitPrice` (decimal(18,2)): Price at time of adding
- `AddedDate` (datetime2): When item was added

**Indexes:**
- Clustered index on `Id`
- Non-clustered index on `CartId`
- Unique index on `CartId, ProductId` (one product per cart)

**Constraints:**
- FK to Carts (CartId) with CASCADE delete
- FK to Products (ProductId)
- Check constraint: Quantity > 0

### Orders

Customer orders with full order lifecycle tracking.

**Columns:**
- `Id` (int, PK): Primary key
- `OrderNumber` (nvarchar(50), unique): Human-readable order ID
- `UserId` (nvarchar(450)): Customer identifier
- `OrderDate` (datetime2): When order was placed
- `Status` (int): OrderStatus enum value
- `TotalAmount` (decimal(18,2)): Order total
- `ShippingAddressId` (int, FK): Delivery address
- `BillingAddressId` (int, FK): Billing address
- `PaymentMethod` (nvarchar(50)): Payment type
- `PaymentTransactionId` (nvarchar(100)): Payment reference
- `ShippingTrackingNumber` (nvarchar(100)): Shipment tracking
- `Notes` (nvarchar(max)): Order notes
- `CreatedDate` (datetime2): Record creation
- `ModifiedDate` (datetime2): Last modification

**Indexes:**
- Clustered index on `Id`
- Unique non-clustered index on `OrderNumber`
- Non-clustered index on `UserId, OrderDate`
- Non-clustered index on `Status`

**Constraints:**
- FK to Addresses (ShippingAddressId)
- FK to Addresses (BillingAddressId)
- Check constraint: TotalAmount >= 0

### OrderItems

Line items within an order.

**Columns:**
- `Id` (int, PK): Primary key
- `OrderId` (int, FK): Reference to Orders
- `ProductId` (int, FK): Reference to Products
- `Quantity` (int): Number of items ordered
- `UnitPrice` (decimal(18,2)): Price at time of order
- `Discount` (decimal(18,2)): Discount applied
- `TotalPrice` (decimal(18,2)): Calculated: (UnitPrice * Quantity) - Discount

**Indexes:**
- Clustered index on `Id`
- Non-clustered index on `OrderId`
- Non-clustered index on `ProductId`

**Constraints:**
- FK to Orders (OrderId) with CASCADE delete
- FK to Products (ProductId)
- Check constraint: Quantity > 0
- Check constraint: UnitPrice >= 0
- Check constraint: Discount >= 0

### Addresses

Shipping and billing addresses.

**Columns:**
- `Id` (int, PK): Primary key
- `UserId` (nvarchar(450)): Owner of address
- `FirstName` (nvarchar(100)): Recipient first name
- `LastName` (nvarchar(100)): Recipient last name
- `Street` (nvarchar(200)): Street address
- `City` (nvarchar(100)): City
- `State` (nvarchar(100)): State/Province
- `PostalCode` (nvarchar(20)): ZIP/Postal code
- `Country` (nvarchar(100)): Country
- `Phone` (nvarchar(20)): Contact phone
- `IsDefault` (bit): Default address for user
- `CreatedDate` (datetime2): Record creation

**Indexes:**
- Clustered index on `Id`
- Non-clustered index on `UserId`
- Non-clustered index on `UserId, IsDefault`

### StockMovements

Inventory tracking and audit trail.

**Columns:**
- `Id` (int, PK): Primary key
- `ProductId` (int, FK): Reference to Products
- `Quantity` (int): Change in quantity (can be negative)
- `MovementType` (int): StockMovementType enum
- `ReferenceId` (nvarchar(100)): Reference to source (OrderId, etc.)
- `Notes` (nvarchar(500)): Additional information
- `Timestamp` (datetime2): When movement occurred
- `UserId` (nvarchar(450)): Who performed the movement

**Movement Types:**
- Purchase: Stock received from supplier
- Sale: Stock sold to customer
- Adjustment: Manual inventory correction
- Return: Customer return
- Damage: Damaged/unsellable items

**Indexes:**
- Clustered index on `Id`
- Non-clustered index on `ProductId, Timestamp`
- Non-clustered index on `MovementType`

**Constraints:**
- FK to Products (ProductId)

**Current Stock Calculation:**
```sql
SELECT ProductId, SUM(Quantity) as CurrentStock
FROM StockMovements
GROUP BY ProductId
```

### ImportJobs

File import job tracking and history.

**Columns:**
- `Id` (int, PK): Primary key
- `FileName` (nvarchar(255)): Original file name
- `FileType` (nvarchar(50)): File format (Excel, JSON, XML)
- `FilePath` (nvarchar(500)): Storage path
- `Status` (int): ImportJobStatus enum
- `UploadedDate` (datetime2): When file was uploaded
- `ProcessedDate` (datetime2, nullable): When processing completed
- `RowsProcessed` (int): Successfully processed rows
- `RowsFailed` (int): Failed rows
- `ErrorLog` (nvarchar(max)): Error details (JSON)
- `UserId` (nvarchar(450)): Who uploaded the file

**Indexes:**
- Clustered index on `Id`
- Non-clustered index on `Status, UploadedDate`
- Non-clustered index on `UserId`

## Entity Framework Core Configuration

### DbContext Setup

**ApplicationDbContext.cs:**

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<ImportJob> ImportJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

### Entity Configurations

Using IEntityTypeConfiguration for clean separation:

**ProductConfiguration.cs:**

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.SKU)
            .IsUnique();

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => new { p.IsActive, p.Price });
    }
}
```

### Indexing Strategy

**Performance Indexes:**
- Unique indexes on natural keys (SKU, OrderNumber)
- Foreign key indexes for JOIN optimization
- Composite indexes for common queries
- Covering indexes for frequently accessed columns

**Example Composite Index:**
```csharp
builder.HasIndex(p => new { p.IsActive, p.CategoryId, p.Price })
    .HasFilter("IsActive = 1");
```

## Migration Strategy

### Creating Migrations

**Initial Migration:**
```bash
cd src/EcomShopping.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../EcomShopping.API
```

**Subsequent Migrations:**
```bash
# After modifying entity models
dotnet ef migrations add AddProductImageGallery --startup-project ../EcomShopping.API
```

### Reviewing Migrations

Always review generated migrations before applying:

```bash
# View migration SQL without applying
dotnet ef migrations script --startup-project ../EcomShopping.API

# View specific migration range
dotnet ef migrations script PreviousMigration NextMigration --startup-project ../EcomShopping.API
```

### Applying Migrations

**Development:**
```bash
dotnet ef database update --startup-project ../EcomShopping.API
```

**Production:**
```bash
# Generate SQL script for DBA review
dotnet ef migrations script --startup-project ../EcomShopping.API --output migration.sql --idempotent

# Apply via SQL tools after approval
```

### Rolling Back Migrations

```bash
# Rollback to specific migration
dotnet ef database update PreviousMigration --startup-project ../EcomShopping.API

# Remove last migration (if not applied)
dotnet ef migrations remove --startup-project ../EcomShopping.API
```

## Data Seeding

### Seed Data Strategy

**Development Seed Data:**
- Sample categories
- Test products
- Demo orders

**Production Seed Data:**
- Essential categories
- System configuration
- Default settings

### Implementation

**SeedData.cs:**

```csharp
public static class SeedData
{
    public static void Initialize(ApplicationDbContext context)
    {
        // Check if data exists
        if (context.Categories.Any())
        {
            return; // Database has been seeded
        }

        // Seed categories
        var categories = new[]
        {
            new Category { Name = "Electronics", Description = "Electronic devices", IsActive = true },
            new Category { Name = "Clothing", Description = "Apparel and accessories", IsActive = true },
            new Category { Name = "Books", Description = "Books and publications", IsActive = true }
        };
        context.Categories.AddRange(categories);
        context.SaveChanges();

        // Seed products
        var products = new[]
        {
            new Product 
            { 
                Name = "Laptop", 
                SKU = "ELEC-001", 
                Price = 999.99M, 
                CategoryId = categories[0].Id,
                IsActive = true
            },
            // ... more products
        };
        context.Products.AddRange(products);
        context.SaveChanges();
    }
}
```

**Call in Program.cs:**

```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    if (app.Environment.IsDevelopment())
    {
        context.Database.EnsureCreated();
        SeedData.Initialize(context);
    }
}
```

## Database Maintenance

### Backup Strategy

**Development:**
- Daily automatic backups
- Keep last 7 days

**Production:**
- Hourly transaction log backups
- Daily full backups
- Weekly differential backups
- Monthly archives

### Performance Monitoring

**Key Metrics:**
- Query execution time
- Index usage statistics
- Missing index recommendations
- Deadlock monitoring
- Connection pool usage

**SQL Server Queries:**

```sql
-- Find missing indexes
SELECT * FROM sys.dm_db_missing_index_details

-- Find unused indexes
SELECT * FROM sys.dm_db_index_usage_stats
WHERE user_seeks = 0 AND user_scans = 0

-- Check query performance
SELECT TOP 10 
    total_worker_time/execution_count AS avg_cpu_time,
    total_elapsed_time/execution_count AS avg_elapsed_time,
    text
FROM sys.dm_exec_query_stats
CROSS APPLY sys.dm_exec_sql_text(sql_handle)
ORDER BY avg_cpu_time DESC
```

### Index Maintenance

**Rebuild Strategy:**
- Rebuild indexes with fragmentation > 30%
- Reorganize indexes with fragmentation 10-30%
- Update statistics weekly

**Maintenance Script:**
```sql
-- Rebuild fragmented indexes
ALTER INDEX ALL ON Products REBUILD

-- Update statistics
UPDATE STATISTICS Products WITH FULLSCAN
```

## Migration Best Practices

1. **Version Control**: All migrations in source control
2. **Review**: Always review generated SQL before applying
3. **Testing**: Test migrations on staging before production
4. **Idempotent Scripts**: Use `--idempotent` flag for production
5. **Rollback Plan**: Always have rollback migration ready
6. **Backup**: Take backup before applying migrations
7. **Monitoring**: Monitor performance after schema changes
8. **Documentation**: Document complex migrations

## Schema Evolution Strategy

### Adding New Tables

1. Create new entity class
2. Add DbSet to DbContext
3. Create configuration class
4. Generate migration
5. Review and apply

### Modifying Existing Tables

1. Update entity class
2. Generate migration
3. Review SQL for data preservation
4. Add custom migration code if needed
5. Test thoroughly

### Deleting Tables/Columns

1. Mark as obsolete first
2. Deploy without deleting
3. Monitor usage
4. Create deletion migration
5. Apply after verification

## Troubleshooting

### Common Issues

**Migration Conflicts:**
```bash
# When two developers create migrations simultaneously
# Merge migrations or recreate with consistent order
dotnet ef migrations remove
# Coordinate with team, then recreate
```

**Connection Issues:**
```bash
# Verify connection string
dotnet ef dbcontext info --startup-project ../EcomShopping.API

# Test connection
dotnet ef database update --startup-project ../EcomShopping.API --verbose
```

**Performance Issues:**
- Add missing indexes
- Review query execution plans
- Consider denormalization for read-heavy operations
- Implement caching for frequently accessed data

## Future Enhancements

1. **Partitioning**: Table partitioning for large tables (Orders, StockMovements)
2. **Read Replicas**: Separate read and write databases
3. **Archiving**: Move old orders to archive database
4. **Full-Text Search**: Implement for product search
5. **Temporal Tables**: Track all historical changes
6. **Compression**: Enable data compression on large tables

## Conclusion

The database schema is designed for:
- **Performance**: Strategic indexing and query optimization
- **Scalability**: Can handle growth through partitioning and replication
- **Maintainability**: Clean migrations and version control
- **Reliability**: Proper constraints and relationships
- **Flexibility**: Easy to extend with new features

The Entity Framework Core code-first approach provides a solid foundation for evolving the schema as business requirements change.
