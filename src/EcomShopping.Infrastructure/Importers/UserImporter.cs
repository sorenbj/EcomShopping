using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.FileImport.Core;
using System.Security.Cryptography;
using System.Text;

namespace EcomShopping.Infrastructure.Importers;

/// <summary>
/// Importer for User table
/// </summary>
public class UserImporter : ITableImporter
{
    private readonly IUserRepository _userRepository;

    public UserImporter(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public string TableName => "Users";

    public List<FieldMapping> GetDefaultFieldMappings()
    {
        return new List<FieldMapping>
        {
            new() { SourceField = "Email", DestinationField = "Email", IsRequired = true },
            new() { SourceField = "UserName", DestinationField = "UserName", IsRequired = true },
            new() { SourceField = "Password", DestinationField = "Password", IsRequired = true },
            new() { SourceField = "FirstName", DestinationField = "FirstName", IsRequired = true },
            new() { SourceField = "LastName", DestinationField = "LastName", IsRequired = true },
            new() { SourceField = "PhoneNumber", DestinationField = "PhoneNumber", IsRequired = false },
            new() { SourceField = "IsActive", DestinationField = "IsActive", IsRequired = false,
                DefaultValue = "true",
                Transform = obj => Convert.ToBoolean(obj.ToString()) },
            new() { SourceField = "EmailConfirmed", DestinationField = "EmailConfirmed", IsRequired = false,
                DefaultValue = "false",
                Transform = obj => Convert.ToBoolean(obj.ToString()) },
        };
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateRecordAsync(Dictionary<string, object> record)
    {
        // Validate required fields
        if (!record.ContainsKey("Email") || string.IsNullOrWhiteSpace(record["Email"]?.ToString()))
        {
            return (false, "Email is required");
        }

        if (!record.ContainsKey("UserName") || string.IsNullOrWhiteSpace(record["UserName"]?.ToString()))
        {
            return (false, "UserName is required");
        }

        if (!record.ContainsKey("Password") || string.IsNullOrWhiteSpace(record["Password"]?.ToString()))
        {
            return (false, "Password is required");
        }

        if (!record.ContainsKey("FirstName") || string.IsNullOrWhiteSpace(record["FirstName"]?.ToString()))
        {
            return (false, "FirstName is required");
        }

        if (!record.ContainsKey("LastName") || string.IsNullOrWhiteSpace(record["LastName"]?.ToString()))
        {
            return (false, "LastName is required");
        }

        // Validate email format
        var email = record["Email"].ToString()!;
        if (!IsValidEmail(email))
        {
            return (false, "Invalid email format");
        }

        // Validate email uniqueness
        var existingUserByEmail = await _userRepository.GetByEmailAsync(email);
        if (existingUserByEmail != null)
        {
            return (false, $"User with email '{email}' already exists");
        }

        // Validate username uniqueness
        var userName = record["UserName"].ToString()!;
        var existingUserByUserName = await _userRepository.GetByUserNameAsync(userName);
        if (existingUserByUserName != null)
        {
            return (false, $"User with username '{userName}' already exists");
        }

        // Validate password strength (basic check)
        var password = record["Password"].ToString()!;
        if (password.Length < 6)
        {
            return (false, "Password must be at least 6 characters long");
        }

        return (true, null);
    }

    public async Task<ImportRecordResult> ImportRecordAsync(
        Dictionary<string, object> record,
        ImportConfiguration configuration)
    {
        try
        {
            var user = new User
            {
                Email = record["Email"].ToString()!,
                UserName = record["UserName"].ToString()!,
                PasswordHash = HashPassword(record["Password"].ToString()!),
                FirstName = record["FirstName"].ToString()!,
                LastName = record["LastName"].ToString()!,
                PhoneNumber = record.ContainsKey("PhoneNumber") && !string.IsNullOrWhiteSpace(record["PhoneNumber"]?.ToString())
                    ? record["PhoneNumber"]?.ToString()
                    : null,
                IsActive = record.ContainsKey("IsActive")
                    ? Convert.ToBoolean(record["IsActive"].ToString())
                    : true,
                EmailConfirmed = record.ContainsKey("EmailConfirmed")
                    ? Convert.ToBoolean(record["EmailConfirmed"].ToString())
                    : false
            };

            var createdUser = await _userRepository.AddAsync(user);

            return new ImportRecordResult
            {
                Success = true,
                SourceData = record,
                ImportedEntity = createdUser
            };
        }
        catch (Exception ex)
        {
            return new ImportRecordResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                SourceData = record
            };
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Hashes a password using SHA256
    /// Note: In production, use a proper password hashing library like BCrypt or ASP.NET Core Identity
    /// </summary>
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
