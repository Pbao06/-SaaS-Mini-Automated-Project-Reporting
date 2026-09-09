using Microsoft.AspNetCore.Mvc;
using SaaS.Api.Models.DTOs.Admin;
using SaaS.Api.Services.Admin;

namespace SaaS.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUserController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUserController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers(CancellationToken ct)
        {
            try
            {
                var users = await _adminUserService.GetAllUsersAsync(ct);
                return Ok(new
                {
                    Success = true,
                    Message = "Lấy danh sách người dùng thành công",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy danh sách người dùng",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("{id:guid}/toggle-ban")]
        public async Task<IActionResult> ToggleBanUser(Guid id, CancellationToken ct)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { Success = false, Message = "Id người dùng không hợp lệ" });

                var result = await _adminUserService.ToggleBanUserAsync(id, ct);
                if (!result)
                    return NotFound(new { Success = false, Message = "Không tìm thấy người dùng" });

                return Ok(new { Success = true, Message = "Cập nhật trạng thái người dùng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi cập nhật trạng thái người dùng",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("{id:guid}/role")]
        public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleRequest request, CancellationToken ct)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { Success = false, Message = "Id người dùng không hợp lệ" });

                if (string.IsNullOrWhiteSpace(request.RoleId))
                    return BadRequest(new { Success = false, Message = "RoleId không hợp lệ" });

                var result = await _adminUserService.AssignRoleAsync(id, request.RoleId, ct);
                if (!result)
                    return NotFound(new { Success = false, Message = "Không tìm thấy người dùng hoặc role không tồn tại" });

                return Ok(new { Success = true, Message = "Gán vai trò thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi gán vai trò",
                    Error = ex.Message
                });
            }
        }
    }
}
