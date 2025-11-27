using Xunit;
using FluentAssertions;
using EcomShopping.Domain.Entities;

namespace EcomShopping.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void User_Create_ShouldHaveDefaultValues()
    {
        // Act
        var user = new User();

        // Assert
        user.IsActive.Should().BeTrue();
        user.EmailConfirmed.Should().BeFalse();
        user.UserRoles.Should().NotBeNull();
        user.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void User_SetProperties_ShouldUpdateCorrectly()
    {
        // Arrange
        var user = new User();
        var now = DateTime.UtcNow;

        // Act
        user.Email = "test@example.com";
        user.UserName = "testuser";
        user.FirstName = "Test";
        user.LastName = "User";
        user.PhoneNumber = "123-456-7890";
        user.CreatedAt = now;
        user.IsActive = true;
        user.EmailConfirmed = true;

        // Assert
        user.Email.Should().Be("test@example.com");
        user.UserName.Should().Be("testuser");
        user.FirstName.Should().Be("Test");
        user.LastName.Should().Be("User");
        user.PhoneNumber.Should().Be("123-456-7890");
        user.CreatedAt.Should().Be(now);
        user.IsActive.Should().BeTrue();
        user.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public void User_AddRole_ShouldAddToUserRoles()
    {
        // Arrange
        var user = new User { Id = 1 };
        var role = new Role { Id = 1, Name = "Admin" };
        var userRole = new UserRole 
        { 
            UserId = user.Id, 
            RoleId = role.Id, 
            User = user, 
            Role = role,
            AssignedAt = DateTime.UtcNow
        };

        // Act
        user.UserRoles.Add(userRole);

        // Assert
        user.UserRoles.Should().HaveCount(1);
        user.UserRoles.First().Role.Name.Should().Be("Admin");
    }
}
