using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using SaaS.Api.Data;
using SaaS.Api.DTOs.Common;
using SaaS.Api.DTOs.Users;
using SaaS.Api.Enum;
using SaaS.Api.Models;
using SaaS.Api.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;

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

        public async Task<ServicesResponse> RegisterAsync(RegisterAccountRequest registerRequest)
        {
            //Email da duoc dang ky
            var accountExists = await _dbContext.Users.AnyAsync(u => u.Email == registerRequest.Email.Trim().ToLowerInvariant());
            if (accountExists)
                return ServicesResponse.ErrorResponse("Email này đã được đăng ký", ResultStatus.ValidationError);
            //Ma Otp da duoc su dung
            var otpCodeOfEmail = await _dbContext.OtpCodes.AsNoTracking().FirstOrDefaultAsync(o => o.Email == registerRequest.Email);
            var VerifiedTokenRequestHash = HashToken(registerRequest.VerifiedToken);
            if (otpCodeOfEmail == null || otpCodeOfEmail.UsedAt == null || otpCodeOfEmail.VerifiedToken != VerifiedTokenRequestHash)
                return ServicesResponse.ErrorResponse("Email chưa xác thực hoặc token không hợp lệ",ResultStatus.Conflict);
            //Neu token het han
            if (otpCodeOfEmail.VerifiedTokenDate == null || otpCodeOfEmail.VerifiedTokenDate <= DateTime.UtcNow)
                return ServicesResponse.ErrorResponse("Phiên xác thực đã kết thúc.Vui lòng xác thực lại email",ResultStatus.Conflict);
            var newUser = new User()
            {
                Id = Guid.NewGuid(),
                Email = registerRequest.Email,
                Role = await _dbContext.Roles.FirstOrDefaultAsync(r=>r.Id == "User") ?? null!,
                FullName = registerRequest.Fullname,
            };
            var hasher = new PasswordHasher<User>();
            newUser.PasswordHash = hasher.HashPassword(newUser, registerRequest.Password);
            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();
            return ServicesResponse.SuccessResponse("Đăng ký thành công");
        }

        public async Task<ServicesResponse> SendRegistrationOtpAsync(SendOtpRequest request,CancellationToken ct)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
                return ServicesResponse.ErrorResponse("Email đã tồn tại", Enum.ResultStatus.Error);
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                //Neu da gui yeu cau roi va muon gui lai se update thay vi them moi 1 ban ghi otpcode
                var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
                var passwordHasher = new PasswordHasher<OtpCode>();
                var otpHash = passwordHasher.HashPassword(null!, otp);
                var oldOtpsUpdate = await _dbContext.OtpCodes.Where(o=>o.Email == request.Email).ExecuteUpdateAsync(s=>s
                .SetProperty(o=>o.ExpiresAt,DateTime.UtcNow.AddMinutes(5))
                .SetProperty(o=>o.Attempts,0)
                .SetProperty(o=>o.CodeHash,otpHash));
                //Neu khong co otp cu thi tao otp
                if (oldOtpsUpdate == 0)
                {
                    //Neu mail hop le thi gui otp
                    var newOtpCode = new OtpCode();
                    newOtpCode.Email = request.Email;
                    newOtpCode.Id = Guid.NewGuid().ToString();
                    newOtpCode.CodeHash = otpHash;
                    newOtpCode.Purpose = OtpCodePurpose.CreateAccount;
                    newOtpCode.ExpiresAt = DateTime.UtcNow.AddMinutes(5);
                    _dbContext.OtpCodes.Add(newOtpCode);
                    //Luu otp vao db
                    await _dbContext.SaveChangesAsync();
                }
                //Gui otp
                await _emailServices.SendOtpAsync(request.Email, otp,ct);
                //Chi commit thi gui mail thanh cong
                await transaction.CommitAsync();
                return ServicesResponse.SuccessResponse($"Đã gửi OTP đến email : {request.Email}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine(ex.ToString());
                return ServicesResponse.ErrorResponse("Gửi OTP thất bại", Enum.ResultStatus.Error);
            }
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
        public async Task<ServicesResponse> VerifyRegistrationOtpAsync(VerifyOtpRequest request)
        {
            var AttemptsLimit = 5;
            var email = request.Email.Trim().ToLowerInvariant();
            try
            {
                //Atomic tang Attemps theo dieu kien de tranh Race Condition (Goi Request verify 2 lan cung luc)
                var claimed = await _dbContext.OtpCodes
                    .Where(o => o.Email == email
                                && o.UsedAt == null
                                && o.Attempts < AttemptsLimit
                                && o.ExpiresAt > DateTime.UtcNow)
                    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Attempts, o => o.Attempts + 1));

                var otpCodeInDb = await _dbContext.OtpCodes.AsNoTracking()
                      .FirstOrDefaultAsync(o => o.Email == email);
                if (claimed == 0)
                {
                    //Email chua duoc gui OTP
                    if (otpCodeInDb == null)
                        return ServicesResponse.ErrorResponse("Email chưa được gửi OTP", ResultStatus.ValidationError);

                    //Vuot qua so lan thu
                    if (otpCodeInDb.Attempts >= AttemptsLimit)
                        return ServicesResponse.ErrorResponse("Số lần nhập OTP quá mức cho phép", ResultStatus.Error);

                    if (otpCodeInDb.UsedAt != null)
                        return ServicesResponse.ErrorResponse("Mã Otp đã được sử dụng", ResultStatus.Error);


                    var RemainingAttempts = AttemptsLimit - otpCodeInDb.Attempts;
                    if (otpCodeInDb.ExpiresAt <= DateTime.UtcNow)
                        return ServicesResponse.ErrorResponse($"Otp quá hạn sử dụng", ResultStatus.Error);
                }

                //So sanh voi mat khau duoc bam trong db
                var _passwordHasher = new PasswordHasher<OtpCode>();
                var verifyResult = _passwordHasher.VerifyHashedPassword(null!, otpCodeInDb!.CodeHash, request.Otp);
                var isValid = verifyResult == PasswordVerificationResult.Success || verifyResult == PasswordVerificationResult.SuccessRehashNeeded;

                //Sai otp
                if (!isValid)
                {
                    var remaining = AttemptsLimit - otpCodeInDb.Attempts;
                    return ServicesResponse.ErrorResponse($"Sai OTP, ban còn {remaining} lần thử", ResultStatus.ValidationError);
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
                    .SetProperty(o=>o.VerifiedToken,tokenHash));
                if (confirmed == 0)
                {
                    // Request khác đã verify thành công và claim UsedAt trước, ngay giữa lúc B2 và B3
                    return ServicesResponse.ErrorResponse("Email đã được xác thực", ResultStatus.Error);
                }

                return ServicesResponse.SuccessResponse("Xác thực OTP thành công");
            }
            catch (Exception)
            {
                return ServicesResponse.ErrorResponse("Da xay ra loi khi xac thuc otp",ResultStatus.Error);
            }
        }
    }
}
