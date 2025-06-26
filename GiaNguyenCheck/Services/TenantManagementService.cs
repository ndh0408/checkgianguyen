using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GiaNguyenCheck.Data;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.DTOs;
using GiaNguyenCheck.Interfaces;
using GiaNguyenCheck.Constants;
using System.Security.Cryptography;
using System.Text;

namespace GiaNguyenCheck.Services
{
    /// <summary>
    /// Service quản lý tenant và mã mời (chỉ SystemAdmin được sử dụng)
    /// </summary>
    public class TenantManagementService : ITenantManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<TenantManagementService> _logger;

        public TenantManagementService(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IEmailService emailService,
            ILogger<TenantManagementService> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo tenant mới (chỉ SystemAdmin)
        /// </summary>
        public async Task<ApiResponse<TenantDto>> CreateTenantAsync(RegisterTenantDto dto)
        {
            try
            {
                // Kiểm tra subdomain đã tồn tại chưa
                var existingTenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.Subdomain.ToLower() == dto.Subdomain.ToLower());
                
                if (existingTenant != null)
                {
                    return ApiResponse<TenantDto>.ErrorResult("Subdomain đã được sử dụng");
                }

                // Kiểm tra email admin đã tồn tại chưa
                var existingUser = await _userManager.FindByEmailAsync(dto.AdminEmail);
                if (existingUser != null)
                {
                    return ApiResponse<TenantDto>.ErrorResult("Email admin đã được sử dụng");
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // 1. Tạo tenant
                    var tenant = new Tenant
                    {
                        Name = dto.TenantName,
                        Description = dto.TenantDescription,
                        Email = dto.TenantEmail,
                        Phone = dto.TenantPhone,
                        Address = dto.TenantAddress,
                        Subdomain = dto.Subdomain.ToLower(),
                        CurrentPlan = (ServicePlan)dto.ServicePlan,
                        PlanExpiryDate = dto.PlanExpiryDate ?? DateTime.UtcNow.AddDays(30),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Tenants.Add(tenant);
                    await _context.SaveChangesAsync();

                    // 2. Tạo admin user cho tenant
                    var adminUser = new User
                    {
                        UserName = dto.AdminEmail,
                        Email = dto.AdminEmail,
                        FirstName = dto.AdminFirstName,
                        LastName = dto.AdminLastName,
                        TenantId = tenant.Id,
                        Role = UserRole.TenantAdmin,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createResult = await _userManager.CreateAsync(adminUser, dto.AdminPassword);
                    if (!createResult.Succeeded)
                    {
                        var errors = createResult.Errors.Select(e => e.Description).ToList();
                        return ApiResponse<TenantDto>.ErrorResult("Không thể tạo admin user", errors);
                    }

                    // 3. Gán role TenantAdmin
                    await _userManager.AddToRoleAsync(adminUser, AppConstants.Roles.TenantAdmin);

                    await transaction.CommitAsync();

                    // 4. Gửi email chào mừng
                    try
                    {
                        // await _emailService.SendTenantWelcomeEmailAsync(tenant, adminUser);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Không thể gửi email chào mừng cho tenant {TenantId}", tenant.Id);
                    }

                    var tenantDto = new TenantDto
                    {
                        Id = tenant.Id,
                        Name = tenant.Name,
                        Description = tenant.Description,
                        Email = tenant.Email,
                        Phone = tenant.Phone,
                        Address = tenant.Address,
                        Subdomain = tenant.Subdomain,
                        CurrentPlan = tenant.CurrentPlan.ToString(),
                        PlanExpiryDate = tenant.PlanExpiryDate,
                        IsActive = tenant.IsActive,
                        CreatedAt = tenant.CreatedAt
                    };

                    return ApiResponse<TenantDto>.SuccessResult(tenantDto, "Tạo tenant thành công");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo tenant {TenantName}", dto.TenantName);
                return ApiResponse<TenantDto>.ErrorResult("Có lỗi xảy ra khi tạo tenant");
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả tenant (chỉ SystemAdmin)
        /// </summary>
        public async Task<ApiResponse<PagedResult<TenantDto>>> GetTenantsAsync(int page = 1, int pageSize = 20, string? search = null)
        {
            try
            {
                var query = _context.Tenants.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(t => t.Name.Contains(search) || 
                                           t.Email.Contains(search) || 
                                           t.Subdomain.Contains(search));
                }

                var total = await query.CountAsync();
                var tenants = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TenantDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        Email = t.Email,
                        Phone = t.Phone,
                        Address = t.Address,
                        Subdomain = t.Subdomain,
                        CurrentPlan = t.CurrentPlan.ToString(),
                        PlanExpiryDate = t.PlanExpiryDate,
                        IsActive = t.IsActive,
                        CreatedAt = t.CreatedAt
                    })
                    .ToListAsync();

                var result = new PagedResult<TenantDto>
                {
                    Items = tenants,
                    TotalCount = total,
                    Page = page,
                    PageSize = pageSize
                };

                return ApiResponse<PagedResult<TenantDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tenant");
                return ApiResponse<PagedResult<TenantDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách tenant");
            }
        }

        /// <summary>
        /// Tạo mã mời cho user mới (TenantAdmin)
        /// </summary>
        public async Task<ApiResponse<InvitationDto>> CreateInvitationAsync(CreateInvitationDto dto, int tenantId, int createdByUserId)
        {
            try
            {
                // Kiểm tra email đã tồn tại trong tenant chưa
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && u.TenantId == tenantId);
                
                if (existingUser != null)
                {
                    return ApiResponse<InvitationDto>.ErrorResult("Email đã tồn tại trong tổ chức");
                }

                // Kiểm tra đã có mã mời chưa sử dụng cho email này chưa
                var existingInvitation = await _context.Invitations
                    .FirstOrDefaultAsync(i => i.Email == dto.Email && 
                                            i.TenantId == tenantId && 
                                            !i.IsUsed && 
                                            i.ExpiresAt > DateTime.UtcNow);
                
                if (existingInvitation != null)
                {
                    return ApiResponse<InvitationDto>.ErrorResult("Đã có mã mời chưa sử dụng cho email này");
                }

                // Tạo mã mời
                var code = GenerateInvitationCode();
                var expiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddDays(7);

                var invitation = new Invitation
                {
                    Email = dto.Email,
                    Code = code,
                    Role = (UserRole)dto.Role,
                    Message = dto.Message,
                    ExpiresAt = expiresAt,
                    TenantId = tenantId,
                    CreatedByUserId = createdByUserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Invitations.Add(invitation);
                await _context.SaveChangesAsync();

                // Gửi email mời
                try
                {
                    // await _emailService.SendInvitationEmailAsync(invitation);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể gửi email mời cho {Email}", dto.Email);
                }

                var createdByUser = await _context.Users.FindAsync(createdByUserId);
                var invitationDto = new InvitationDto
                {
                    Id = invitation.Id,
                    Email = invitation.Email,
                    Role = (int)invitation.Role,
                    RoleName = invitation.Role.ToString(),
                    Code = invitation.Code,
                    IsUsed = invitation.IsUsed,
                    CreatedAt = invitation.CreatedAt,
                    ExpiresAt = invitation.ExpiresAt,
                    Message = invitation.Message,
                    CreatedByName = createdByUser?.FullName ?? "Unknown"
                };

                return ApiResponse<InvitationDto>.SuccessResult(invitationDto, "Tạo mã mời thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo mã mời cho {Email}", dto.Email);
                return ApiResponse<InvitationDto>.ErrorResult("Có lỗi xảy ra khi tạo mã mời");
            }
        }

        /// <summary>
        /// Validate mã mời
        /// </summary>
        public async Task<ApiResponse<InvitationDto>> ValidateInvitationAsync(string code)
        {
            try
            {
                var invitation = await _context.Invitations
                    .Include(i => i.CreatedByUser)
                    .FirstOrDefaultAsync(i => i.Code == code);

                if (invitation == null)
                {
                    return ApiResponse<InvitationDto>.ErrorResult("Mã mời không tồn tại");
                }

                if (invitation.IsUsed)
                {
                    return ApiResponse<InvitationDto>.ErrorResult("Mã mời đã được sử dụng");
                }

                if (invitation.ExpiresAt < DateTime.UtcNow)
                {
                    return ApiResponse<InvitationDto>.ErrorResult("Mã mời đã hết hạn");
                }

                var invitationDto = new InvitationDto
                {
                    Id = invitation.Id,
                    Email = invitation.Email,
                    Role = (int)invitation.Role,
                    RoleName = invitation.Role.ToString(),
                    Code = invitation.Code,
                    IsUsed = invitation.IsUsed,
                    CreatedAt = invitation.CreatedAt,
                    ExpiresAt = invitation.ExpiresAt,
                    Message = invitation.Message,
                    CreatedByName = invitation.CreatedByUser?.FullName ?? "Unknown"
                };

                return ApiResponse<InvitationDto>.SuccessResult(invitationDto, "Mã mời hợp lệ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi validate mã mời {Code}", code);
                return ApiResponse<InvitationDto>.ErrorResult("Có lỗi xảy ra khi kiểm tra mã mời");
            }
        }

        /// <summary>
        /// Đánh dấu mã mời đã được sử dụng
        /// </summary>
        public async Task<bool> MarkInvitationAsUsedAsync(string code, int usedByUserId)
        {
            try
            {
                var invitation = await _context.Invitations
                    .FirstOrDefaultAsync(i => i.Code == code && !i.IsUsed);

                if (invitation == null) return false;

                invitation.IsUsed = true;
                invitation.UsedAt = DateTime.UtcNow;
                invitation.UsedByUserId = usedByUserId;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu mã mời {Code} đã sử dụng", code);
                return false;
            }
        }

        /// <summary>
        /// Tạo mã mời ngẫu nhiên
        /// </summary>
        private static string GenerateInvitationCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var result = new char[32];
            
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            
            for (int i = 0; i < 32; i++)
            {
                result[i] = chars[bytes[i] % chars.Length];
            }
            
            return new string(result);
        }
    }
} 