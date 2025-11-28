using EcomShopping.Infrastructure.Data;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Repositories;
using EcomShopping.Infrastructure.Payment;
using EcomShopping.Infrastructure.Services;
using EcomShopping.Infrastructure.Importers;
using EcomShopping.Domain.Entities;
using EcomShopping.Integration.Core;
using EcomShopping.Integration.Core.Providers;
using EcomShopping.FileImport.Core;
using EcomShopping.FileImport.Core.Parsers;
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
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", false);

if (useInMemory)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("EcomShoppingDb"));
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
builder.Services.AddScoped<IStockReservationRepository, StockReservationRepository>();
builder.Services.AddScoped<ILowStockEventRepository, LowStockEventRepository>();
builder.Services.AddScoped<IImportJobRepository, ImportJobRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// Register services
builder.Services.AddScoped<IPaymentProvider, FakePaymentProvider>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<InventoryService>();

// Register file import services
builder.Services.AddScoped<IFileParser, ExcelFileParser>();
builder.Services.AddScoped<IFileParser, JsonFileParser>();
builder.Services.AddScoped<IFileParser, XmlFileParser>();
builder.Services.AddScoped<ITableImporter, ProductImporter>();
builder.Services.AddScoped<ITableImporter, CategoryImporter>();
builder.Services.AddScoped<FileImportService>();
builder.Services.AddScoped<FileImportOrchestrationService>();

// Register integration services
builder.Services.AddSingleton<IntegrationProviderRegistry>();
builder.Services.AddSingleton<IntegrationEngine>();
builder.Services.AddSingleton<IntegrationScheduler>();

// Register mock integration providers
builder.Services.AddSingleton(sp =>
{
    var registry = sp.GetRequiredService<IntegrationProviderRegistry>();
    
    // Register mock providers
    registry.Register("erp_provider", new MockErpIntegration());
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

// Seed the database with sample data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Ensure database is created (for SQL Server)
        if (!useInMemory)
        {
            context.Database.EnsureCreated();
        }
        
        // Seed data for both in-memory and SQL Server
        DbSeeder.SeedDatabase(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
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

