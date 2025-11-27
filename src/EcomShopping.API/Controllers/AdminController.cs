using EcomShopping.Application.DTOs;
using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcomShopping.API.Controllers;

/// <summary>
/// API endpoints for admin-specific operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ILogger<AdminController> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    // User Management Endpoints

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of all users</returns>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = users.Select(MapToUserDto).ToList();
            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    /// <summary>
    /// Get a specific user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("users/{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            var user = await _userRepository.GetWithRolesAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(MapToUserDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="dto">User creation data</param>
    /// <returns>Created user</returns>
    [HttpPost("users")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
    {
        try
        {
            // Check if email already exists
            var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingEmail != null)
            {
                return BadRequest("Email already exists");
            }

            // Check if username already exists
            var existingUsername = await _userRepository.GetByUserNameAsync(dto.UserName);
            if (existingUsername != null)
            {
                return BadRequest("Username already exists");
            }

            // In a real application, you would hash the password properly
            // For now, we'll use a simple placeholder (NOT SECURE - FOR DEMO ONLY)
            var user = new User
            {
                Email = dto.Email,
                UserName = dto.UserName,
                PasswordHash = HashPassword(dto.Password), // Simple hash for demo
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                IsActive = true,
                EmailConfirmed = false
            };

            var createdUser = await _userRepository.AddAsync(user);
            var userDto = MapToUserDto(createdUser);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "An error occurred while creating the user");
        }
    }

    /// <summary>
    /// Update a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="dto">User update data</param>
    /// <returns>No content</returns>
    [HttpPut("users/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Email = dto.Email;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;
            user.IsActive = dto.IsActive;

            await _userRepository.UpdateAsync(user);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, "An error occurred while updating the user");
        }
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content</returns>
    [HttpDelete("users/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, "An error occurred while deleting the user");
        }
    }

    // Role Management Endpoints

    /// <summary>
    /// Get all roles
    /// </summary>
    /// <returns>List of all roles</returns>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
    {
        try
        {
            var roles = await _roleRepository.GetAllAsync();
            var roleDtos = roles.Select(MapToRoleDto).ToList();
            return Ok(roleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, "An error occurred while retrieving roles");
        }
    }

    /// <summary>
    /// Get a specific role by ID
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Role details</returns>
    [HttpGet("roles/{id}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleDto>> GetRole(int id)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(MapToRoleDto(role));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role {RoleId}", id);
            return StatusCode(500, "An error occurred while retrieving the role");
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    /// <param name="role">Role data</param>
    /// <returns>Created role</returns>
    [HttpPost("roles")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleDto>> CreateRole(Role role)
    {
        try
        {
            // Check if role name already exists
            var existingRole = await _roleRepository.GetByNameAsync(role.Name);
            if (existingRole != null)
            {
                return BadRequest("Role name already exists");
            }

            var createdRole = await _roleRepository.AddAsync(role);
            var roleDto = MapToRoleDto(createdRole);
            return CreatedAtAction(nameof(GetRole), new { id = createdRole.Id }, roleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, "An error occurred while creating the role");
        }
    }

    /// <summary>
    /// Update a role
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="role">Role update data</param>
    /// <returns>No content</returns>
    [HttpPut("roles/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRole(int id, Role role)
    {
        try
        {
            if (id != role.Id)
            {
                return BadRequest("Role ID mismatch");
            }

            var existingRole = await _roleRepository.GetByIdAsync(id);
            if (existingRole == null)
            {
                return NotFound();
            }

            await _roleRepository.UpdateAsync(role);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            return StatusCode(500, "An error occurred while updating the role");
        }
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>No content</returns>
    [HttpDelete("roles/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRole(int id)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            await _roleRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            return StatusCode(500, "An error occurred while deleting the role");
        }
    }

    /// <summary>
    /// Get users by role
    /// </summary>
    /// <param name="roleName">Role name</param>
    /// <returns>List of users with the specified role</returns>
    [HttpGet("roles/{roleName}/users")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(string roleName)
    {
        try
        {
            var users = await _userRepository.GetUsersByRoleAsync(roleName);
            var userDtos = users.Select(MapToUserDto).ToList();
            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users by role {RoleName}", roleName);
            return StatusCode(500, "An error occurred while retrieving users by role");
        }
    }

    // Dashboard/Statistics Endpoints

    /// <summary>
    /// Get admin dashboard statistics
    /// </summary>
    /// <returns>Dashboard statistics</returns>
    [HttpGet("dashboard/stats")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetDashboardStats(
        [FromServices] IProductRepository productRepository,
        [FromServices] IOrderRepository orderRepository,
        [FromServices] IImportJobRepository importJobRepository)
    {
        try
        {
            var products = await productRepository.GetAllAsync();
            var orders = await orderRepository.GetAllAsync();
            var importJobs = await importJobRepository.GetRecentJobsAsync(10);
            var users = await _userRepository.GetAllAsync();

            var stats = new
            {
                totalProducts = products.Count(),
                activeProducts = products.Count(p => p.IsActive),
                lowStockProducts = products.Count(p => p.IsActive && p.StockQuantity <= 10),
                totalOrders = orders.Count(),
                pendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                processingOrders = orders.Count(o => o.Status == OrderStatus.Processing),
                shippedOrders = orders.Count(o => o.Status == OrderStatus.Shipped),
                deliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
                totalRevenue = orders.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount),
                recentImportJobs = importJobs.Count(),
                pendingImports = importJobs.Count(j => j.Status == ImportJobStatus.Pending),
                totalUsers = users.Count(),
                activeUsers = users.Count(u => u.IsActive)
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return StatusCode(500, "An error occurred while retrieving dashboard stats");
        }
    }

    // Helper Methods

    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>()
        };
    }

    private RoleDto MapToRoleDto(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        };
    }

    /// <summary>
    /// INSECURE password hashing for demonstration purposes ONLY.
    /// 
    /// CRITICAL SECURITY WARNING: This implementation is NOT production-ready!
    /// SHA256 without salt is vulnerable to:
    /// - Rainbow table attacks
    /// - Dictionary attacks
    /// - No key stretching (fast computation allows brute force)
    /// 
    /// For production use, implement one of these secure alternatives:
    /// 1. ASP.NET Core Identity (recommended for full auth system)
    /// 2. BCrypt.Net library (bcrypt algorithm)
    /// 3. Argon2 (modern, memory-hard algorithm)
    /// 4. PBKDF2 with high iteration count and unique salt per password
    /// 
    /// Example with BCrypt:
    ///   using BCrypt.Net;
    ///   var hashedPassword = BCrypt.HashPassword(password);
    ///   var isValid = BCrypt.Verify(password, hashedPassword);
    /// </summary>
    private string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
