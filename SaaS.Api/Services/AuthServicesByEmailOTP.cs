using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaaS.Api.Data;
using SaaS.Api.DTOs.Common;
using SaaS.Api.DTOs.Users;
using SaaS.Api.Enum;
using SaaS.Api.Enums;
using SaaS.Api.Models;
using SaaS.Api.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SaaS.Api.Services
{
    public class AuthServicesByEmailOTP : IAuthServices
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly IEmailServices _emailServices;
        private readonly IConfiguration _configuration;

        public AuthServicesByEmailOTP(ApplicationDBContext dbContext, IEmailServices emailServices, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _emailServices = emailServices;
            _configuration = configuration;
        }

        public async Task<ServicesResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email, ct);

            if (user is null)
                return ServicesResponse<LoginResponse>.ErrorResponse("Email hoặc mật khẩu không đúng", ResultStatus.Unauthorized);

            if (!user.IsActive)
                return ServicesResponse<LoginResponse>.ErrorResponse("Tài khoản chưa được kích hoạt", ResultStatus.Forbidden);

            var hasher = new PasswordHasher<User>();
            var verifyResult = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (verifyResult == PasswordVerificationResult.Failed)
                return ServicesResponse<LoginResponse>.ErrorResponse("Email hoặc mật khẩu không đúng", ResultStatus.Unauthorized);

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken(out var refreshTokenHash);

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _dbContext.RefreshTokens.Add(refreshTokenEntity);
            await _dbContext.SaveChangesAsync(ct);

            return ServicesResponse<LoginResponse>.SuccessResponse(
                new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30)
                },
                "Đăng nhập thành công");
        }

        public async Task<ServicesResponse> LogoutAsync(Guid userId, CancellationToken ct)
        {
            var activeTokens = await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(ct);

            foreach (var token in activeTokens)
                token.IsRevoked = true;

            await _dbContext.SaveChangesAsync(ct);
            return ServicesResponse.SuccessResponse("Đăng xuất thành công");
        }

        public Task Logout()
        {
            throw new NotImplementedException();
        }

        public async Task<ServicesResponse> RegisterAsync(RegisterAccountRequest registerRequest, CancellationToken ct)
        {
            var email = registerRequest.Email.Trim().ToLowerInvariant();

            if (await _dbContext.Users.AnyAsync(u => u.Email == email, ct))
                return ServicesResponse.ErrorResponse("Email này đã được đăng ký", ResultStatus.ValidationError);

            if (!IsPasswordStrong(registerRequest.Password))
                return ServicesResponse.ErrorResponse("Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt", ResultStatus.ValidationError);

            var otpCodeOfEmail = await _dbContext.OtpCodes.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Email == email, ct);

            var VerifiedTokenRequestHash = HashToken(registerRequest.VerifiedToken);

            if (otpCodeOfEmail == null || otpCodeOfEmail.UsedAt == null || otpCodeOfEmail.VerifiedToken != VerifiedTokenRequestHash)
                return ServicesResponse.ErrorResponse("Email chưa xác thực hoặc token không hợp lệ", ResultStatus.Conflict);

            if (otpCodeOfEmail.VerifiedTokenDate == null || otpCodeOfEmail.VerifiedTokenDate <= DateTime.UtcNow)
                return ServicesResponse.ErrorResponse("Phiên xác thực đã kết thúc. Vui lòng xác thực lại email", ResultStatus.Conflict);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                RoleId = "User",
                FullName = registerRequest.Fullname.Trim(),
                IsActive = true
            };

            var hasher = new PasswordHasher<User>();
            newUser.PasswordHash = hasher.HashPassword(newUser, registerRequest.Password);

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync(ct);

            return ServicesResponse.SuccessResponse("Đăng ký thành công");
        }

        public async Task<ServicesResponse> SendRegistrationOtpAsync(SendOtpRequest request, CancellationToken ct)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (await _dbContext.Users.AnyAsync(u => u.Email == email, ct))
                return ServicesResponse.ErrorResponse("Email đã tồn tại", ResultStatus.Error);

            var oldOtps = await _dbContext.OtpCodes
                .Where(o => o.Email == email && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(ct);

            if (oldOtps.Count > 0 && DateTime.UtcNow - oldOtps[0].CreatedAt < TimeSpan.FromMinutes(1))
                return ServicesResponse.ErrorResponse("Vui lòng chờ 1 phút trước khi gửi lại OTP", ResultStatus.Error);

            foreach (var old in oldOtps)
                old.ExpiresAt = DateTime.UtcNow;

            var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
            var passwordHasher = new PasswordHasher<OtpCode>();
            var otpHash = passwordHasher.HashPassword(null!, otp);

            var newOtpCode = new OtpCode
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                CodeHash = otpHash,
                Purpose = OtpCodePurpose.CreateAccount,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Status = OtpCodeSendingStatus.Pending
            };

            _dbContext.OtpCodes.Add(newOtpCode);

            try
            {
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving OTP code to database: {ex.Message}");
                return ServicesResponse.ErrorResponse("Gửi OTP thất bại", ResultStatus.Error);
            }

            bool emailSent = false;
            try
            {
                await _emailServices.SendOtpAsync(email, otp, ct);
                emailSent = true;
            }
            catch (Exception)
            {
                return ServicesResponse.ErrorResponse("Gửi OTP thất bại, vui lòng thử lại", ResultStatus.Error);
            }
            finally
            {
                newOtpCode.Status = emailSent ? OtpCodeSendingStatus.Success : OtpCodeSendingStatus.Failed;

                try
                {
                    await _dbContext.SaveChangesAsync(CancellationToken.None);
                }
                catch
                {
                    // ignored
                }
            }

            return emailSent
                ? ServicesResponse.SuccessResponse("OTP đã được gửi thành công")
                : ServicesResponse.ErrorResponse("Gửi OTP thất bại, vui lòng thử lại", ResultStatus.Error);
        }

        public async Task<ServicesResponse<VerifyOtpResponse>> VerifyRegistrationOtpAsync(VerifyOtpRequest request, CancellationToken ct)
        {
            const int attemptsLimit = 5;
            var email = request.Email.Trim().ToLowerInvariant();

            try
            {
                var claimed = await _dbContext.OtpCodes
                    .Where(o => o.Email == email
                                && o.UsedAt == null
                                && o.Attempts < attemptsLimit
                                && o.ExpiresAt > DateTime.UtcNow)
                    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Attempts, o => o.Attempts + 1), ct);

                var otpCodeInDb = await _dbContext.OtpCodes.AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Email == email && o.ExpiresAt > DateTime.UtcNow, ct);

                if (claimed == 0)
                {
                    if (otpCodeInDb == null)
                        return ServicesResponse<VerifyOtpResponse>.ErrorResponse("Email chưa được gửi OTP", ResultStatus.ValidationError);

                    if (otpCodeInDb.Attempts >= attemptsLimit)
                        return ServicesResponse<VerifyOtpResponse>.ErrorResponse("Số lần nhập OTP quá mức cho phép", ResultStatus.Error);

                    if (otpCodeInDb.UsedAt != null)
                        return ServicesResponse<VerifyOtpResponse>.ErrorResponse("Mã Otp đã được sử dụng", ResultStatus.Error);

                    if (otpCodeInDb.ExpiresAt <= DateTime.UtcNow)
                        return ServicesResponse<VerifyOtpResponse>.ErrorResponse("Otp quá hạn sử dụng", ResultStatus.Error);
                }

                var passwordHasher = new PasswordHasher<OtpCode>();
                var verifyResult = passwordHasher.VerifyHashedPassword(null!, otpCodeInDb!.CodeHash, request.Otp);
                var isValid = verifyResult == PasswordVerificationResult.Success || verifyResult == PasswordVerificationResult.SuccessRehashNeeded;

                if (!isValid)
                {
                    var remaining = attemptsLimit - otpCodeInDb.Attempts;
                    return ServicesResponse<VerifyOtpResponse>.ErrorResponse($"Sai OTP, bạn còn {remaining} lần thử", ResultStatus.ValidationError);
                }

                var rawToken = GenerateSecureToken();
                var tokenHash = HashToken(rawToken);
                var tokenExpiresAt = DateTime.UtcNow.AddMinutes(20);

                var confirmed = await _dbContext.OtpCodes
                    .Where(o => o.Email == email && o.UsedAt == null)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(o => o.UsedAt, DateTime.UtcNow)
                        .SetProperty(o => o.VerifiedTokenDate, tokenExpiresAt)
                        .SetProperty(o => o.VerifiedToken, tokenHash), ct);

                if (confirmed == 0)
                    return ServicesResponse<VerifyOtpResponse>.ErrorResponse("Email đã được xác thực", ResultStatus.Error);

                return ServicesResponse<VerifyOtpResponse>.SuccessResponse(
                    new VerifyOtpResponse { VerifiedToken = rawToken },
                    "Xác thực OTP thành công");
            }
            catch (Exception)
            {
                return ServicesResponse<VerifyOtpResponse>.ErrorResponse("Đã xảy ra lỗi khi xác thực otp", ResultStatus.Error);
            }
        }

        private static string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        private static string GenerateRefreshToken(out string tokenHash)
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(bytes);
            tokenHash = HashToken(token);
            return token;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            var hasUpper = password.Any(char.IsUpper);
            var hasLower = password.Any(char.IsLower);
            var hasDigit = password.Any(char.IsDigit);
            var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }
    }
}
