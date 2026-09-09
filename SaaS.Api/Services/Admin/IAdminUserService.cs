using SaaS.Api.Models.DTOs.Admin;

namespace SaaS.Api.Services.Admin
{
    public interface IAdminUserService
    {
        Task<List<UserDto>> GetAllUsersAsync(CancellationToken ct = default);
        Task<bool> ToggleBanUserAsync(Guid userId, CancellationToken ct = default);
        Task<bool> AssignRoleAsync(Guid userId, string roleId, CancellationToken ct = default);
    }
}
