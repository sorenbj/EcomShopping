using Xunit;
using FluentAssertions;
using Moq;
using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Importers;
using EcomShopping.FileImport.Core;

namespace EcomShopping.UnitTests.Infrastructure;

public class UserImporterTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly UserImporter _importer;

    public UserImporterTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _importer = new UserImporter(_mockUserRepository.Object);
    }

    [Fact]
    public void TableName_ShouldReturnUsers()
    {
        // Act
        var tableName = _importer.TableName;

        // Assert
        tableName.Should().Be("Users");
    }

    [Fact]
    public void GetDefaultFieldMappings_ShouldReturnCorrectMappings()
    {
        // Act
        var mappings = _importer.GetDefaultFieldMappings();

        // Assert
        mappings.Should().NotBeNull();
        mappings.Should().HaveCountGreaterThan(0);
        mappings.Should().Contain(m => m.SourceField == "Email" && m.IsRequired);
        mappings.Should().Contain(m => m.SourceField == "UserName" && m.IsRequired);
        mappings.Should().Contain(m => m.SourceField == "Password" && m.IsRequired);
        mappings.Should().Contain(m => m.SourceField == "FirstName" && m.IsRequired);
        mappings.Should().Contain(m => m.SourceField == "LastName" && m.IsRequired);
        mappings.Should().Contain(m => m.SourceField == "PhoneNumber" && !m.IsRequired);
        mappings.Should().Contain(m => m.SourceField == "IsActive" && !m.IsRequired);
        mappings.Should().Contain(m => m.SourceField == "EmailConfirmed" && !m.IsRequired);
    }

    [Fact]
    public async Task ValidateRecordAsync_ShouldReturnValid_WhenAllRequiredFieldsPresent()
    {
        // Arrange
        var record = new Dictionary<string, object>
        {
            { "Email", "test@example.com" },
            { "UserName", "testuser" },
            { "Password", "SecurePassword123" },
            { "FirstName", "Test" },
            { "LastName", "User" }
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.GetByUserNameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var (isValid, errorMessage) = await _importer.ValidateRecordAsync(record);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Fact]
    public async Task ValidateRecordAsync_ShouldReturnInvalid_WhenEmailMissing()
    {
        // Arrange
        var record = new Dictionary<string, object>
        {
            { "UserName", "testuser" },
            { "Password", "SecurePassword123" },
            { "FirstName", "Test" },
            { "LastName", "User" }
        };

        // Act
        var (isValid, errorMessage) = await _importer.ValidateRecordAsync(record);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Email is required");
    }

    [Fact]
    public async Task ValidateRecordAsync_ShouldReturnInvalid_WhenUserNameMissing()
    {
        // Arrange
        var record = new Dictionary<string, object>
        {
            { "Email", "test@example.com" },
            { "Password", "SecurePassword123" },
            { "FirstName", "Test" },
            { "LastName", "User" }
        };

        // Act
        var (isValid, errorMessage) = await _importer.ValidateRecordAsync(record);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("UserName is required");
    }

    [Fact]
    public async Task ValidateRecordAsync_ShouldReturnInvalid_WhenPasswordMissing()
    {
        // Arrange
        var record = new Dictionary<string, object>
        {
            { "Email", "test@example.com" },
            { "UserName", "testuser" },
            { "FirstName", "Test" },
            { "LastName", "User" }
        };

        // Act
        var (isValid, errorMessage) = await _importer.ValidateRecordAsync(record);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Password is required");
    }

    [Fact]
    public async Task ValidateRecordAsync_ShouldReturnInvalid_WhenEmailFormatInvalid()
    {
        // Arrange
        var record = new Dictionary<string, object>
        {
            { "Email", "invalid-email" },
            { "UserName", "testuser" },
            { "Password", "SecurePassword123" },
            { "FirstName", "Test" },
            { "LastName", "User" }
        };

        // Act
        var (isValid, errorMessage) = await _importer.ValidateRecordAsync(record);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Invalid email format");
    }

    [Fact]
    public async Task ValidateRecordAsync_ShouldReturnInvalid_WhenEmailAlreadyExists()
    {
        // Arrange
        var record = new Dictionary<string, object>
        {
            { "Email", "existing@example.com" },
            { "UserName", "testuser" },
            { "Password", "SecurePassword123" },
            { "FirstName", "Test" },
            { "LastName", "User" }
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync("existing@example.com"))
            .ReturnsAsync(new User { Id = 1, Email = "existing@example.com" });

        // Act
        var (isValid, errorMessage) = await _importer.ValidateRecordAsync(record);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("User with email 'existing@example.com' already exists");
    }

    [Fact]
    public async Task ValidateRecordAsync_ShouldReturnInvalid_WhenUserNameAlreadyExists()
    {
        // Arrange
        var record = new Dictionary<string, object>
        {
            { "Email", "test@example.com" },
            { "UserName", "existinguser" },
            { "Password", "SecurePassword123" },
            { "FirstName", "Test" },
            { "LastName", "User" }
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.GetByUserNameAsync("existinguser"))
            .ReturnsAsync(new User { Id = 1, UserName = "existinguser" });

        // Act
        var (isValid, errorMessage) = await _importer.ValidateRecordAsync(record);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("User with username 'existinguser' already exists");
    }

    [Fact]
    public async Task ValidateRecordAsync_ShouldReturnInvalid_WhenPasswordTooShort()
    {
        // Arrange
        var record = new Dictionary<string, object>
        {
            { "Email", "test@example.com" },
            { "UserName", "testuser" },
            { "Password", "12345" },
            { "FirstName", "Test" },
            { "LastName", "User" }
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.GetByUserNameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var (isValid, errorMessage) = await _importer.ValidateRecordAsync(record);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Password must be at least 6 characters long");
    }

    [Fact]
    public async Task ImportRecordAsync_ShouldCreateUser_WhenValidDataProvided()
    {
        // Arrange
        var record = new Dictionary<string, object>
        {
            { "Email", "newuser@example.com" },
            { "UserName", "newuser" },
            { "Password", "SecurePassword123" },
            { "FirstName", "New" },
            { "LastName", "User" },
            { "PhoneNumber", "+1-555-0199" },
            { "IsActive", true },
            { "EmailConfirmed", false }
        };

        var expectedUser = new User
        {
            Id = 1,
            Email = "newuser@example.com",
            UserName = "newuser",
            FirstName = "New",
            LastName = "User",
            PhoneNumber = "+1-555-0199",
            IsActive = true,
            EmailConfirmed = false
        };

        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(expectedUser);

        var configuration = new ImportConfiguration
        {
            TargetTable = "Users",
            ValidateBeforeImport = true,
            ContinueOnError = true
        };

        // Act
        var result = await _importer.ImportRecordAsync(record, configuration);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ImportedEntity.Should().NotBeNull();
        result.ErrorMessage.Should().BeNull();
        
        _mockUserRepository.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Email == "newuser@example.com" &&
            u.UserName == "newuser" &&
            u.FirstName == "New" &&
            u.LastName == "User" &&
            u.PhoneNumber == "+1-555-0199" &&
            u.IsActive == true &&
            u.EmailConfirmed == false &&
            !string.IsNullOrEmpty(u.PasswordHash)
        )), Times.Once);
    }

    [Fact]
    public async Task ImportRecordAsync_ShouldUseDefaults_WhenOptionalFieldsMissing()
    {
        // Arrange
        var record = new Dictionary<string, object>
        {
            { "Email", "simple@example.com" },
            { "UserName", "simpleuser" },
            { "Password", "SecurePassword123" },
            { "FirstName", "Simple" },
            { "LastName", "User" }
        };

        var expectedUser = new User
        {
            Id = 1,
            Email = "simple@example.com",
            UserName = "simpleuser",
            FirstName = "Simple",
            LastName = "User",
            IsActive = true,
            EmailConfirmed = false
        };

        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(expectedUser);

        var configuration = new ImportConfiguration
        {
            TargetTable = "Users",
            ValidateBeforeImport = true,
            ContinueOnError = true
        };

        // Act
        var result = await _importer.ImportRecordAsync(record, configuration);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        
        _mockUserRepository.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.IsActive == true &&
            u.EmailConfirmed == false &&
            u.PhoneNumber == null
        )), Times.Once);
    }
}
