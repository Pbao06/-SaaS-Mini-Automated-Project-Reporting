using Microsoft.EntityFrameworkCore;
using SaaS.Api.Data;
using SaaS.Api.Models;
using SaaS.Api.Models.DTOs.Admin;

namespace SaaS.Api.Services.Admin
{
    public class AdminUserService : IAdminUserService
    {
        private readonly ApplicationDBContext _dbContext;

        public AdminUserService(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken ct = default)
        {
            try
            {
                var users = await _dbContext.Users
                    .AsNoTracking()
                    .OrderByDescending(u => u.CreatedAt)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FullName = u.FullName,
                        RoleName = u.Role.Name,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync(ct);

                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllUsersAsync: {ex}");
                return new List<UserDto>();
            }
        }

        public async Task<bool> ToggleBanUserAsync(Guid userId, CancellationToken ct = default)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
                if (user == null)
                    return false;

                user.IsActive = !user.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(ct);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ToggleBanUserAsync: {ex}");
                return false;
            }
        }

        public async Task<bool> AssignRoleAsync(Guid userId, string roleId, CancellationToken ct = default)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
                if (user == null)
                    return false;

                var roleExists = await _dbContext.Roles.AnyAsync(r => r.Id == roleId, ct);
                if (!roleExists)
                    return false;

                user.RoleId = roleId;
                user.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(ct);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssignRoleAsync: {ex}");
                return false;
            }
        }
    }
}
