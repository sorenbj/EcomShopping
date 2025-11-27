using EcomShopping.Infrastructure.Data;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Repositories;
using EcomShopping.Infrastructure.Payment;
using EcomShopping.Infrastructure.Services;
using EcomShopping.Domain.Entities;
using EcomShopping.Integration.Core;
using EcomShopping.Integration.Core.Providers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Database
// Use InMemory database for development if SQL Server is not available
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", false);

if (useInMemory || builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("EcomShoppingDb"));
    useInMemory = true; // Force true for seeding check
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=(localdb)\\mssqllocaldb;Database=EcomShoppingDb;Trusted_Connection=true;MultipleActiveResultSets=true";
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// Register repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddScoped<IRepository<Category>, CategoryRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();
builder.Services.AddScoped<IImportJobRepository, ImportJobRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// Register services
builder.Services.AddScoped<IPaymentProvider, FakePaymentProvider>();
builder.Services.AddScoped<CheckoutService>();

// Register integration services
builder.Services.AddSingleton<IntegrationProviderRegistry>();
builder.Services.AddSingleton<IntegrationEngine>();
builder.Services.AddSingleton<IntegrationScheduler>();

// Register mock integration providers
builder.Services.AddSingleton(sp =>
{
    var registry = sp.GetRequiredService<IntegrationProviderRegistry>();
    
    // Register mock providers
    registry.Register("mock-erp", new MockErpIntegration());
    registry.Register("mock-crm", new MockCrmIntegration());
    registry.Register("mock-shipping", new MockShippingProvider());
    registry.Register("mock-payment", new MockPaymentProvider());
    
    return registry;
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "EcomShopping API", 
        Version = "v1",
        Description = "API for managing an e-commerce platform with product catalog, inventory, categories, orders, and cart management"
    });
    
    // Enable XML documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Seed the database with sample data for development
if (useInMemory)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    SeedDatabase(context);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcomShopping API v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();

static void SeedDatabase(ApplicationDbContext context)
{
    // Skip if already seeded
    if (context.Products.Any())
        return;

    // Add categories
    var electronics = new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and gadgets" };
    var clothing = new Category { Id = 2, Name = "Clothing", Description = "Apparel and fashion" };
    var books = new Category { Id = 3, Name = "Books", Description = "Books and literature" };
    var home = new Category { Id = 4, Name = "Home & Garden", Description = "Home and garden products" };

    context.Categories.AddRange(electronics, clothing, books, home);
    context.SaveChanges();

    // Add sample products
    var products = new[]
    {
        new Product
        {
            Id = 1,
            Name = "Wireless Headphones",
            Slug = "wireless-headphones",
            Description = "Premium noise-cancelling wireless headphones with 30-hour battery life",
            Price = 299.99M,
            SKU = "ELEC-WH-001",
            CategoryId = 1,
            StockQuantity = 50,
            IsActive = true,
            Images = new List<string> { "https://via.placeholder.com/400x400?text=Wireless+Headphones" },
            Metadata = new Dictionary<string, string> 
            { 
                { "Brand", "AudioTech" },
                { "Color", "Black" },
                { "Battery Life", "30 hours" },
                { "Connectivity", "Bluetooth 5.0" }
            }
        },
        new Product
        {
            Id = 2,
            Name = "Smart Watch",
            Slug = "smart-watch",
            Description = "Fitness tracking smart watch with heart rate monitor and GPS",
            Price = 399.99M,
            SKU = "ELEC-SW-002",
            CategoryId = 1,
            StockQuantity = 35,
            IsActive = true,
            Images = new List<string> { "https://via.placeholder.com/400x400?text=Smart+Watch" },
            Metadata = new Dictionary<string, string>
            {
                { "Brand", "FitTech" },
                { "Display", "1.4 inch AMOLED" },
                { "Water Resistance", "5 ATM" }
            }
        },
        new Product
        {
            Id = 3,
            Name = "Laptop Stand",
            Slug = "laptop-stand",
            Description = "Ergonomic aluminum laptop stand with adjustable height",
            Price = 49.99M,
            SKU = "ELEC-LS-003",
            CategoryId = 1,
            StockQuantity = 100,
            IsActive = true,
            Images = new List<string> { "https://via.placeholder.com/400x400?text=Laptop+Stand" },
            Metadata = new Dictionary<string, string>
            {
                { "Material", "Aluminum" },
                { "Adjustable", "Yes" },
                { "Weight", "1.2 kg" }
            }
        },
        new Product
        {
            Id = 4,
            Name = "Cotton T-Shirt",
            Slug = "cotton-t-shirt",
            Description = "100% organic cotton t-shirt, comfortable and breathable",
            Price = 29.99M,
            SKU = "CLO-TS-001",
            CategoryId = 2,
            StockQuantity = 200,
            IsActive = true,
            Images = new List<string> { "https://via.placeholder.com/400x400?text=T-Shirt" },
            Metadata = new Dictionary<string, string>
            {
                { "Material", "100% Organic Cotton" },
                { "Sizes", "S, M, L, XL" },
                { "Colors", "White, Black, Navy" }
            }
        },
        new Product
        {
            Id = 5,
            Name = "Running Shoes",
            Slug = "running-shoes",
            Description = "Lightweight running shoes with cushioned sole and breathable mesh",
            Price = 89.99M,
            SKU = "CLO-RS-002",
            CategoryId = 2,
            StockQuantity = 75,
            IsActive = true,
            Images = new List<string> { "https://via.placeholder.com/400x400?text=Running+Shoes" },
            Metadata = new Dictionary<string, string>
            {
                { "Type", "Running" },
                { "Sole Material", "Rubber" },
                { "Upper Material", "Mesh" }
            }
        },
        new Product
        {
            Id = 6,
            Name = "The Great Novel",
            Slug = "the-great-novel",
            Description = "A bestselling fiction novel about adventure and discovery",
            Price = 19.99M,
            SKU = "BOOK-FIC-001",
            CategoryId = 3,
            StockQuantity = 150,
            IsActive = true,
            Images = new List<string> { "https://via.placeholder.com/400x400?text=Book+Cover" },
            Metadata = new Dictionary<string, string>
            {
                { "Author", "Jane Smith" },
                { "Pages", "352" },
                { "Publisher", "Great Books Publishing" },
                { "ISBN", "978-1234567890" }
            }
        },
        new Product
        {
            Id = 7,
            Name = "Cookbook Collection",
            Slug = "cookbook-collection",
            Description = "Comprehensive cookbook with over 500 recipes from around the world",
            Price = 34.99M,
            SKU = "BOOK-COO-002",
            CategoryId = 3,
            StockQuantity = 80,
            IsActive = true,
            Images = new List<string> { "https://via.placeholder.com/400x400?text=Cookbook" },
            Metadata = new Dictionary<string, string>
            {
                { "Author", "Chef Michael Brown" },
                { "Recipes", "500+" },
                { "Cuisine Types", "International" }
            }
        },
        new Product
        {
            Id = 8,
            Name = "Garden Tool Set",
            Slug = "garden-tool-set",
            Description = "Complete 10-piece garden tool set with ergonomic handles",
            Price = 79.99M,
            SKU = "HOME-GTS-001",
            CategoryId = 4,
            StockQuantity = 45,
            IsActive = true,
            Images = new List<string> { "https://via.placeholder.com/400x400?text=Garden+Tools" },
            Metadata = new Dictionary<string, string>
            {
                { "Pieces", "10" },
                { "Material", "Stainless Steel" },
                { "Handle", "Ergonomic Rubber Grip" }
            }
        },
        new Product
        {
            Id = 9,
            Name = "LED Plant Light",
            Slug = "led-plant-light",
            Description = "Full spectrum LED grow light for indoor plants",
            Price = 59.99M,
            SKU = "HOME-LED-002",
            CategoryId = 4,
            StockQuantity = 60,
            IsActive = true,
            Images = new List<string> { "https://via.placeholder.com/400x400?text=Plant+Light" },
            Metadata = new Dictionary<string, string>
            {
                { "Power", "45W" },
                { "Spectrum", "Full Spectrum" },
                { "Coverage", "2x2 feet" }
            }
        },
        new Product
        {
            Id = 10,
            Name = "Wireless Mouse",
            Slug = "wireless-mouse",
            Description = "Ergonomic wireless mouse with precision tracking",
            Price = 24.99M,
            SKU = "ELEC-WM-004",
            CategoryId = 1,
            StockQuantity = 120,
            IsActive = true,
            Images = new List<string> { "https://via.placeholder.com/400x400?text=Wireless+Mouse" },
            Metadata = new Dictionary<string, string>
            {
                { "DPI", "3200" },
                { "Buttons", "6" },
                { "Battery Life", "18 months" }
            }
        }
    };

    context.Products.AddRange(products);
    context.SaveChanges();
}

