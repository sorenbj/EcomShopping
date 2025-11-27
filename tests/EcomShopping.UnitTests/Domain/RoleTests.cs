using Xunit;
using FluentAssertions;
using EcomShopping.Domain.Entities;

namespace EcomShopping.UnitTests.Domain;

public class RoleTests
{
    [Fact]
    public void Role_Create_ShouldHaveDefaultValues()
    {
        // Act
        var role = new Role();

        // Assert
        role.UserRoles.Should().NotBeNull();
        role.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void Role_SetProperties_ShouldUpdateCorrectly()
    {
        // Arrange
        var role = new Role();
        var now = DateTime.UtcNow;

        // Act
        role.Name = "Administrator";
        role.Description = "Full system access";
        role.CreatedAt = now;

        // Assert
        role.Name.Should().Be("Administrator");
        role.Description.Should().Be("Full system access");
        role.CreatedAt.Should().Be(now);
    }

    [Fact]
    public void Role_AddUsers_ShouldAddToUserRoles()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Admin" };
        var user1 = new User { Id = 1, UserName = "user1" };
        var user2 = new User { Id = 2, UserName = "user2" };
        
        var userRole1 = new UserRole 
        { 
            UserId = user1.Id, 
            RoleId = role.Id, 
            User = user1, 
            Role = role,
            AssignedAt = DateTime.UtcNow
        };
        
        var userRole2 = new UserRole 
        { 
            UserId = user2.Id, 
            RoleId = role.Id, 
            User = user2, 
            Role = role,
            AssignedAt = DateTime.UtcNow
        };

        // Act
        role.UserRoles.Add(userRole1);
        role.UserRoles.Add(userRole2);

        // Assert
        role.UserRoles.Should().HaveCount(2);
        role.UserRoles.Select(ur => ur.User.UserName).Should().Contain(new[] { "user1", "user2" });
    }
}
