using System.ComponentModel.DataAnnotations;

namespace EcomShopping.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public List<int> RoleIds { get; set; } = new();
}

public class UpdateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
}

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateRoleDto
{
    [Required(ErrorMessage = "Role name is required")]
    [StringLength(100, ErrorMessage = "Role name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role description is required")]
    [StringLength(500, ErrorMessage = "Role description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;
}

public class UpdateRoleDto
{
    [Required(ErrorMessage = "Role name is required")]
    [StringLength(100, ErrorMessage = "Role name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role description is required")]
    [StringLength(500, ErrorMessage = "Role description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;
}

public class AssignRoleDto
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}
