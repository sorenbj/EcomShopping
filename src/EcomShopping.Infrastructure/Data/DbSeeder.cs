using EcomShopping.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace EcomShopping.Infrastructure.Data;

public static class DbSeeder
{
    public static void SeedDatabase(ApplicationDbContext context)
    {
        // Check if already seeded
        if (context.Products.Any())
        {
            Console.WriteLine("Database already contains data. Skipping seed.");
            return;
        }

        Console.WriteLine("Seeding database with sample data...");

        // Add default roles (let SQL Server auto-generate IDs)
        var adminRole = new Role 
        { 
            Name = "Admin", 
            Description = "Administrator with full access to the admin panel", 
            CreatedAt = DateTime.UtcNow 
        };
        var frontendUserRole = new Role 
        { 
            Name = "FrontendUser", 
            Description = "Regular user with access to the storefront", 
            CreatedAt = DateTime.UtcNow 
        };

        context.Roles.AddRange(adminRole, frontendUserRole);
        context.SaveChanges();
        Console.WriteLine("✓ Roles seeded");

        // Add sample users
        var adminUser = new User
        {
            Email = "admin@ecomshopping.com",
            UserName = "admin",
            PasswordHash = HashPassword("Admin@123"),
            FirstName = "Admin",
            LastName = "User",
            PhoneNumber = "+1-555-0100",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var johnDoe = new User
        {
            Email = "john.doe@example.com",
            UserName = "johndoe",
            PasswordHash = HashPassword("Password@123"),
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1-555-0101",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var janeDoe = new User
        {
            Email = "jane.smith@example.com",
            UserName = "janesmith",
            PasswordHash = HashPassword("Password@123"),
            FirstName = "Jane",
            LastName = "Smith",
            PhoneNumber = "+1-555-0102",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var testUser = new User
        {
            Email = "test@example.com",
            UserName = "testuser",
            PasswordHash = HashPassword("Test@123"),
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+1-555-0103",
            IsActive = true,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(adminUser, johnDoe, janeDoe, testUser);
        context.SaveChanges();
        Console.WriteLine("✓ Users seeded (4 users)");

        // Assign roles to users
        var userRoles = new[]
        {
            new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id, AssignedAt = DateTime.UtcNow },
            new UserRole { UserId = adminUser.Id, RoleId = frontendUserRole.Id, AssignedAt = DateTime.UtcNow },
            new UserRole { UserId = johnDoe.Id, RoleId = frontendUserRole.Id, AssignedAt = DateTime.UtcNow },
            new UserRole { UserId = janeDoe.Id, RoleId = frontendUserRole.Id, AssignedAt = DateTime.UtcNow },
            new UserRole { UserId = testUser.Id, RoleId = frontendUserRole.Id, AssignedAt = DateTime.UtcNow }
        };

        context.UserRoles.AddRange(userRoles);
        context.SaveChanges();
        Console.WriteLine("✓ User roles assigned");

        // Add categories (let SQL Server auto-generate IDs)
        var electronics = new Category { Name = "Electronics", Description = "Electronic devices and gadgets" };
        var clothing = new Category { Name = "Clothing", Description = "Apparel and fashion" };
        var books = new Category { Name = "Books", Description = "Books and literature" };
        var home = new Category { Name = "Home & Garden", Description = "Home and garden products" };

        context.Categories.AddRange(electronics, clothing, books, home);
        context.SaveChanges();
        Console.WriteLine("✓ Categories seeded");

        // Add sample products (let SQL Server auto-generate IDs)
        var products = new[]
        {
            new Product
            {
                Name = "Wireless Headphones",
                Slug = "wireless-headphones",
                Description = "Premium noise-cancelling wireless headphones with 30-hour battery life",
                Price = 299.99M,
                SKU = "ELEC-WH-001",
                CategoryId = electronics.Id,
                StockQuantity = 50,
                LowStockThreshold = 10,
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
                Name = "Smart Watch",
                Slug = "smart-watch",
                Description = "Fitness tracking smart watch with heart rate monitor and GPS",
                Price = 399.99M,
                SKU = "ELEC-SW-002",
                CategoryId = electronics.Id,
                StockQuantity = 35,
                LowStockThreshold = 8,
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
                Name = "Laptop Stand",
                Slug = "laptop-stand",
                Description = "Ergonomic aluminum laptop stand with adjustable height",
                Price = 49.99M,
                SKU = "ELEC-LS-003",
                CategoryId = electronics.Id,
                StockQuantity = 100,
                LowStockThreshold = 20,
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
                Name = "Cotton T-Shirt",
                Slug = "cotton-t-shirt",
                Description = "100% organic cotton t-shirt, comfortable and breathable",
                Price = 29.99M,
                SKU = "CLO-TS-001",
                CategoryId = clothing.Id,
                StockQuantity = 200,
                LowStockThreshold = 30,
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
                Name = "Running Shoes",
                Slug = "running-shoes",
                Description = "Lightweight running shoes with cushioned sole and breathable mesh",
                Price = 89.99M,
                SKU = "CLO-RS-002",
                CategoryId = clothing.Id,
                StockQuantity = 75,
                LowStockThreshold = 15,
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
                Name = "The Great Novel",
                Slug = "the-great-novel",
                Description = "A bestselling fiction novel about adventure and discovery",
                Price = 19.99M,
                SKU = "BOOK-FIC-001",
                CategoryId = books.Id,
                StockQuantity = 150,
                LowStockThreshold = 25,
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
                Name = "Cookbook Collection",
                Slug = "cookbook-collection",
                Description = "Comprehensive cookbook with over 500 recipes from around the world",
                Price = 34.99M,
                SKU = "BOOK-COO-002",
                CategoryId = books.Id,
                StockQuantity = 80,
                LowStockThreshold = 12,
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
                Name = "Garden Tool Set",
                Slug = "garden-tool-set",
                Description = "Complete 10-piece garden tool set with ergonomic handles",
                Price = 79.99M,
                SKU = "HOME-GTS-001",
                CategoryId = home.Id,
                StockQuantity = 45,
                LowStockThreshold = 10,
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
                Name = "LED Plant Light",
                Slug = "led-plant-light",
                Description = "Full spectrum LED grow light for indoor plants",
                Price = 59.99M,
                SKU = "HOME-LED-002",
                CategoryId = home.Id,
                StockQuantity = 60,
                LowStockThreshold = 12,
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
                Name = "Wireless Mouse",
                Slug = "wireless-mouse",
                Description = "Ergonomic wireless mouse with precision tracking",
                Price = 24.99M,
                SKU = "ELEC-WM-004",
                CategoryId = electronics.Id,
                StockQuantity = 120,
                LowStockThreshold = 25,
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
        Console.WriteLine("✓ Products seeded (10 items)");

        // Add sample coupon
        var coupon = new Coupon
        {
            Code = "WELCOME10",
            Description = "10% off for new customers",
            Type = CouponType.Percentage,
            Value = 10M,
            MinimumOrderAmount = 50M,
            IsActive = true,
            ValidFrom = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddMonths(3),
            UsageLimit = 100,
            UsageCount = 0
        };

        context.Coupons.Add(coupon);
        context.SaveChanges();
        Console.WriteLine("✓ Coupons seeded");

        Console.WriteLine("✅ Database seeding completed successfully!");
        Console.WriteLine("\n📋 Sample User Credentials:");
        Console.WriteLine("   Admin User:");
        Console.WriteLine("     Email: admin@ecomshopping.com");
        Console.WriteLine("     Password: Admin@123");
        Console.WriteLine("   Regular Users:");
        Console.WriteLine("     Email: john.doe@example.com | Password: Password@123");
        Console.WriteLine("     Email: jane.smith@example.com | Password: Password@123");
        Console.WriteLine("     Email: test@example.com | Password: Test@123");
    }

    /// <summary>
    /// Simple password hashing using SHA256 (for demo purposes only)
    /// In production, use a proper password hashing library like BCrypt or ASP.NET Core Identity
    /// </summary>
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
