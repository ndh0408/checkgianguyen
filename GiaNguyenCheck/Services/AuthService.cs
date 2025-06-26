using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.DTOs;
using GiaNguyenCheck.Interfaces;
using GiaNguyenCheck.Constants;
using Microsoft.Extensions.Configuration;

namespace GiaNguyenCheck.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITenantProvider _tenantProvider;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ITenantProvider tenantProvider,
            IConfiguration configuration,
            IEmailService emailService,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tenantProvider = tenantProvider;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResultDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null || !user.IsActive)
                {
                    return ApiResponse<LoginResultDto>.ErrorResult("Email hoặc mật khẩu không đúng");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!result.Succeeded)
                {
                    return ApiResponse<LoginResultDto>.ErrorResult("Email hoặc mật khẩu không đúng");
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                var roles = await _userManager.GetRolesAsync(user);
                
                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt,
                    TenantId = user.TenantId,
                    Roles = roles.ToList()
                };
                
                var loginResult = new LoginResultDto
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    User = userDto
                };

                return ApiResponse<LoginResultDto>.SuccessResult(loginResult, "Đăng nhập thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng nhập cho user {Email}", loginDto.Email);
                return ApiResponse<LoginResultDto>.ErrorResult("Có lỗi xảy ra khi đăng nhập");
            }
        }

        public async Task<ApiResponse<UserDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<UserDto>.ErrorResult("Email đã được sử dụng");
                }

                // TODO: Validate invitation code
                // For now, we'll skip invitation validation in this version
                // In production, you should validate the invitation code here

                // Create new user
                var user = new User
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    TenantId = _tenantProvider.GetTenantId() // Get from current context
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<UserDto>.ErrorResult("Không thể tạo tài khoản", errors);
                }

                // Add default role
                await _userManager.AddToRoleAsync(user, AppConstants.Roles.Staff);

                // Send confirmation email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // await _emailService.SendWelcomeEmailAsync(user, null); // TODO: Implement

                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    TenantId = user.TenantId,
                    Roles = new List<string> { AppConstants.Roles.Staff }
                };

                return ApiResponse<UserDto>.SuccessResult(userDto, "Đăng ký thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký user {Email}", registerDto.Email);
                return ApiResponse<UserDto>.ErrorResult("Có lỗi xảy ra khi đăng ký");
            }
        }

        public async Task<ApiResponse<LoginResultDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                // TODO: Implement refresh token validation logic
                // For now, return error
                return ApiResponse<LoginResultDto>.ErrorResult("Refresh token không hợp lệ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi refresh token");
                return ApiResponse<LoginResultDto>.ErrorResult("Có lỗi xảy ra khi refresh token");
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync(int userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    await _signInManager.SignOutAsync();
                    _logger.LogInformation("User {UserId} đã đăng xuất", userId);
                }
                
                return ApiResponse<bool>.SuccessResult(true, "Đăng xuất thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng xuất user {UserId}", userId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi đăng xuất");
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("Người dùng không tồn tại");
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<bool>.ErrorResult("Không thể thay đổi mật khẩu", errors);
                }

                return ApiResponse<bool>.SuccessResult(true, "Thay đổi mật khẩu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi mật khẩu cho user {UserId}", userId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi thay đổi mật khẩu");
            }
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
                if (user == null)
                {
                    // Don't reveal that user doesn't exist
                    return ApiResponse<bool>.SuccessResult(true, "Nếu email tồn tại, bạn sẽ nhận được link reset mật khẩu");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                // await _emailService.SendPasswordResetEmailAsync(user, token); // TODO: Implement

                return ApiResponse<bool>.SuccessResult(true, "Email reset mật khẩu đã được gửi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email reset mật khẩu cho {Email}", forgotPasswordDto.Email);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi gửi email reset");
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("Email không tồn tại");
                }

                var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<bool>.ErrorResult("Không thể reset mật khẩu", errors);
                }

                return ApiResponse<bool>.SuccessResult(true, "Reset mật khẩu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi reset mật khẩu cho {Email}", resetPasswordDto.Email);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi reset mật khẩu");
            }
        }

        public async Task<ApiResponse<UserDto>> GetCurrentUserAsync()
        {
            try
            {
                var userId = _tenantProvider.GetCurrentUserId();
                if (userId == null)
                {
                    return ApiResponse<UserDto>.ErrorResult("Không thể xác định user hiện tại");
                }

                var user = await _userManager.FindByIdAsync(userId.ToString()!);
                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResult("Người dùng không tồn tại");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt,
                    TenantId = user.TenantId,
                    Roles = roles.ToList()
                };

                return ApiResponse<UserDto>.SuccessResult(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin user hiện tại");
                return ApiResponse<UserDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin user");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "")
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "supersecret"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
} 