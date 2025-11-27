namespace EcomShopping.Domain.Entities;

/// <summary>
/// Join table for many-to-many relationship between Users and Roles
/// </summary>
public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public DateTime AssignedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
