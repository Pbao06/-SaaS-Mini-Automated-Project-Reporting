using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaaS.Api.Data;
using SaaS.Api.DTOs.Common;
using SaaS.Api.DTOs.Users;
using SaaS.Api.Enum;
using SaaS.Api.Models;
using SaaS.Api.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
namespace SaaS.Api.Services
{
    public class AuthServicesByEmailOTP : IAuthServices
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly IEmailServices _emailServices;
        public AuthServicesByEmailOTP(ApplicationDBContext dbContext, IEmailServices emailServices)
        {
            _dbContext = dbContext;
            _emailServices = emailServices;
        }

        public Task Login()
        {
            throw new NotImplementedException();
        }

        public Task Logout()
        {
            throw new NotImplementedException();
        }

        public async Task<ServicesResponse> RegisterAsync(RegisterAccountRequest registerRequest,CancellationToken ct)
        {
            //Email da duoc dang ky
            var accountExists = await _dbContext.Users.AnyAsync(u => u.Email == registerRequest.Email.Trim().ToLowerInvariant(),ct);
            if (accountExists)
                return ServicesResponse.ErrorResponse("Email này đã được đăng ký", ResultStatus.ValidationError);
            //Ma Otp da duoc su dung
            var otpCodeOfEmail = await _dbContext.OtpCodes.AsNoTracking().FirstOrDefaultAsync(o => o.Email == registerRequest.Email,ct);
            var VerifiedTokenRequestHash = HashToken(registerRequest.VerifiedToken);
            if (otpCodeOfEmail == null || otpCodeOfEmail.UsedAt == null || otpCodeOfEmail.VerifiedToken != VerifiedTokenRequestHash)
                return ServicesResponse.ErrorResponse("Email chưa xác thực hoặc token không hợp lệ",ResultStatus.Conflict);
            //Neu token het han
            if (otpCodeOfEmail.VerifiedTokenDate == null || otpCodeOfEmail.VerifiedTokenDate <= DateTime.UtcNow)
                return ServicesResponse.ErrorResponse("Phiên xác thực đã kết thúc.Vui lòng xác thực lại email",ResultStatus.Conflict);
            var newUser = new User()
            {
                Id = Guid.NewGuid(),
                Email = registerRequest.Email.Trim().ToLowerInvariant(),
                Role = await _dbContext.Roles.FirstOrDefaultAsync(r=>r.Id == "User", ct) ?? null!,
                FullName = registerRequest.Fullname,
                IsActive = false,
                
            };
            var hasher = new PasswordHasher<User>();
            newUser.PasswordHash = hasher.HashPassword(newUser, registerRequest.Password);
            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync(ct);
            return ServicesResponse.SuccessResponse("Đăng ký thành công");
        }

        public async Task<ServicesResponse> SendRegistrationOtpAsync(SendOtpRequest request,CancellationToken ct)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email,ct))
                return ServicesResponse.ErrorResponse("Email đã tồn tại", Enum.ResultStatus.Error);
            var oldOtps = await _dbContext.OtpCodes
                .Where(o => o.Email == request.Email && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(ct);

            if (oldOtps.Count > 0 && DateTime.UtcNow - oldOtps[0].CreatedAt < TimeSpan.FromMinutes(1))
                return ServicesResponse.ErrorResponse("Vui lòng chờ 1 phút trước khi gửi lại OTP", Enum.ResultStatus.Error);

            foreach (var old in oldOtps)
                old.ExpiresAt = DateTime.UtcNow; // vô hiệu hóa OTP cũ
            //Tao Otp moi va gui mail
            var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var passwordHasher = new PasswordHasher<OtpCode>();
            var otpHash = passwordHasher.HashPassword(null!, otp);

            var newOtpCode = new OtpCode
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                CodeHash = otpHash,
                Purpose = OtpCodePurpose.CreateAccount,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Status = Enums.OtpCodeSendingStatus.Pending,
            };

            _dbContext.OtpCodes.Add(newOtpCode);

            //Luu otp vao db
            try
            {
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ServicesResponse.ErrorResponse("Gửi OTP thất bại", Enum.ResultStatus.Error);
            }
            bool emailSent = false;
            try
            {
                //Gui otp qua mail 
                await _emailServices.SendOtpAsync(request.Email, otp, ct);
                emailSent = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ServicesResponse.ErrorResponse("Gửi OTP thất bại, vui lòng thử lại", Enum.ResultStatus.Error);
            }
            finally
            {
                if (emailSent)
                {
                    newOtpCode.Status = Enums.OtpCodeSendingStatus.Success;
                }
                else
                {
                    newOtpCode.Status = Enums.OtpCodeSendingStatus.Failed;
                }

                try
                {
                    await _dbContext.SaveChangesAsync(CancellationToken.None);
                }
                catch (Exception saveEx)
                {
                    Console.WriteLine(saveEx.ToString());
                }
            }
            //Neu gui mail thanh cong tra ve success, nguoc lai tra ve error
            return emailSent ? ServicesResponse.SuccessResponse("OTP đã được gửi thành công")
                             : ServicesResponse.ErrorResponse("Gửi OTP thất bại, vui lòng thử lại", Enum.ResultStatus.Error);
        }

        private static string GenerateSecureToken()
        {
            // 32 byte = 256-bit entropy, đủ mạnh để không đoán được
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-").Replace("/", "_").TrimEnd('='); // url-safe nếu cần nhét vào query param
        }
        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
        public async Task<ServicesResponse<VerifyOtpResponse>> VerifyRegistrationOtpAsync(VerifyOtpRequest request,CancellationToken ct)
        {
            const int attemptsLimit = 5;
            var email = request.Email.Trim().ToLowerInvariant();
            try
            {
                //Atomic tang Attemps theo dieu kien de tranh Race Condition (Goi Request verify 2 lan cung luc)
                var claimed = await _dbContext.OtpCodes
                    .Where(o => o.Email == email
                                && o.UsedAt == null
                                && o.Attempts < attemptsLimit
                                && o.ExpiresAt > DateTime.UtcNow)
                    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Attempts, o => o.Attempts + 1), ct);
                //Tim Otp cua email trong db
                var otpCodeInDb = await _dbContext.OtpCodes.AsNoTracking()
                      .FirstOrDefaultAsync(o => o.Email == email && o.ExpiresAt > DateTime.UtcNow, ct);
                if (claimed == 0)
                {
                    //Email chua duoc gui OTP
                    if (otpCodeInDb == null)
                        return ServicesResponse<VerifyOtpResponse>.ErrorResponse("Email chưa được gửi OTP", ResultStatus.ValidationError);

                    //Vuot qua so lan thu
                    if (otpCodeInDb.Attempts >= attemptsLimit)
                        return ServicesResponse<VerifyOtpResponse>.ErrorResponse("Số lần nhập OTP quá mức cho phép", ResultStatus.Error);

                    if (otpCodeInDb.UsedAt != null)
                        return ServicesResponse<VerifyOtpResponse>.ErrorResponse("Mã Otp đã được sử dụng", ResultStatus.Error);

                    if (otpCodeInDb.ExpiresAt <= DateTime.UtcNow)
                        return ServicesResponse<VerifyOtpResponse>.ErrorResponse($"Otp quá hạn sử dụng", ResultStatus.Error);
                }

                //So sanh voi mat khau duoc bam trong db
                var _passwordHasher = new PasswordHasher<OtpCode>();
                var verifyResult = _passwordHasher.VerifyHashedPassword(null!, otpCodeInDb!.CodeHash, request.Otp);
                var isValid = verifyResult == PasswordVerificationResult.Success || verifyResult == PasswordVerificationResult.SuccessRehashNeeded;

                //Sai otp
                if (!isValid)
                {
                    var remaining = attemptsLimit - otpCodeInDb.Attempts;
                    return ServicesResponse<VerifyOtpResponse>.ErrorResponse($"Sai OTP, ban còn {remaining} lần thử", ResultStatus.ValidationError);
                }
                //Dung Otp cap nhat otp la da dung
                var rawToken = GenerateSecureToken();
                var tokenHash = HashToken(rawToken);
                var tokenExpiresAt = DateTime.UtcNow.AddMinutes(20);
                var confirmed = await _dbContext.OtpCodes
                    .Where(o => o.Email == email && o.UsedAt == null)
                    .ExecuteUpdateAsync(s=>s
                    .SetProperty(o=>o.UsedAt,DateTime.UtcNow)
                    .SetProperty(o=>o.VerifiedTokenDate,tokenExpiresAt)
                    .SetProperty(o=>o.VerifiedToken,tokenHash), ct);
                if (confirmed == 0)
                {
                    // Request khác đã verify thành công và claim UsedAt trước, ngay giữa lúc B2 và B3
                    return ServicesResponse<VerifyOtpResponse>.ErrorResponse("Email đã được xác thực", ResultStatus.Error);
                }
                var VerifyOtpResponse = new VerifyOtpResponse()
                {
                    VerifiedToken = rawToken,
                };
                return ServicesResponse<VerifyOtpResponse>.SuccessResponse(VerifyOtpResponse,"Xác thực OTP thành công");
            }
            catch (Exception)
            {
                return ServicesResponse<VerifyOtpResponse>.ErrorResponse("Da xay ra loi khi xac thuc otp",ResultStatus.Error);
            }
        }
    }
}
