using EcomShopping.Infrastructure.Data;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Repositories;
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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=EcomShoppingDb;Trusted_Connection=true;MultipleActiveResultSets=true";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IRepository<Category>, CategoryRepository>();

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
    c.SwaggerDoc("v1", new() { Title = "EcomShopping API", Version = "v1" });
});

var app = builder.Build();

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
