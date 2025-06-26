using GiaNguyenCheck.DTOs;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;
using GiaNguyenCheck.Repositories;
using GiaNguyenCheck.Constants;

namespace GiaNguyenCheck.Services
{
    public class TenantService : ITenantService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IEmailService _emailService;

        public TenantService(
            ITenantRepository tenantRepository,
            IUserRepository userRepository,
            ITenantProvider tenantProvider,
            IEmailService emailService)
        {
            _tenantRepository = tenantRepository;
            _userRepository = userRepository;
            _tenantProvider = tenantProvider;
            _emailService = emailService;
        }

        public async Task<ApiResponse<TenantDto>> CreateTenantAsync(CreateTenantDto createTenantDto)
        {
            try
            {
                // Check if subdomain already exists
                var existingTenant = await _tenantRepository.GetBySubdomainAsync(createTenantDto.Subdomain);
                if (existingTenant != null)
                {
                    return ApiResponse<TenantDto>.Error("Subdomain already exists");
                }

                // Check if email already exists
                var existingUser = await _userRepository.GetByEmailAsync(createTenantDto.AdminEmail);
                if (existingUser != null)
                {
                    return ApiResponse<TenantDto>.Error("Email already registered");
                }

                // Create tenant
                var tenant = new Tenant
                {
                    Name = createTenantDto.Name,
                    Subdomain = createTenantDto.Subdomain,
                    Email = createTenantDto.AdminEmail,
                    Phone = createTenantDto.PhoneNumber,
                    Address = createTenantDto.Address,
                    CurrentPlan = ServicePlan.Free,
                    PlanExpiryDate = DateTime.UtcNow.AddYears(1),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdTenant = await _tenantRepository.CreateAsync(tenant);
                if (createdTenant == null)
                {
                    return ApiResponse<TenantDto>.Error("Failed to create tenant");
                }

                // Create admin user
                var adminUser = new User
                {
                    UserName = createTenantDto.AdminEmail,
                    Email = createTenantDto.AdminEmail,
                    FirstName = createTenantDto.AdminFirstName,
                    LastName = createTenantDto.AdminLastName,
                    TenantId = createdTenant.Id,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var userResult = await _userRepository.CreateAsync(adminUser, createTenantDto.AdminPassword);
                if (!userResult.IsSuccess)
                {
                    // Rollback tenant creation
                    await _tenantRepository.DeleteAsync(createdTenant.Id);
                    return ApiResponse<TenantDto>.Error(userResult.Message);
                }

                // Assign admin role
                await _userRepository.AssignRoleAsync(adminUser.Id, AppConstants.Roles.TenantAdmin);

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(adminUser, createdTenant.Name);

                var tenantDto = new TenantDto
                {
                    Id = createdTenant.Id,
                    Name = createdTenant.Name,
                    Subdomain = createdTenant.Subdomain,
                    Email = createdTenant.Email,
                    Phone = createdTenant.Phone,
                    Address = createdTenant.Address,
                    CurrentPlan = createdTenant.CurrentPlan.ToString(),
                    PlanExpiryDate = createdTenant.PlanExpiryDate,
                    IsActive = createdTenant.IsActive,
                    CreatedAt = createdTenant.CreatedAt
                };

                return ApiResponse<TenantDto>.Success(tenantDto, "Tenant created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<TenantDto>.Error($"Error creating tenant: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TenantDto>> GetTenantAsync(int tenantId)
        {
            try
            {
                var tenant = await _tenantRepository.GetByIdAsync(tenantId);
                if (tenant == null)
                {
                    return ApiResponse<TenantDto>.Error("Tenant not found");
                }

                var tenantDto = new TenantDto
                {
                    Id = tenant.Id,
                    Name = tenant.Name,
                    Subdomain = tenant.Subdomain,
                    Email = tenant.Email,
                    Phone = tenant.Phone,
                    Address = tenant.Address,
                    CurrentPlan = tenant.CurrentPlan.ToString(),
                    PlanExpiryDate = tenant.PlanExpiryDate,
                    IsActive = tenant.IsActive,
                    CreatedAt = tenant.CreatedAt
                };

                return ApiResponse<TenantDto>.Success(tenantDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<TenantDto>.Error($"Error retrieving tenant: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TenantDto>> GetTenantBySubdomainAsync(string subdomain)
        {
            try
            {
                var tenant = await _tenantRepository.GetBySubdomainAsync(subdomain);
                if (tenant == null)
                {
                    return ApiResponse<TenantDto>.Error("Tenant not found");
                }

                var tenantDto = new TenantDto
                {
                    Id = tenant.Id,
                    Name = tenant.Name,
                    Subdomain = tenant.Subdomain,
                    Email = tenant.Email,
                    Phone = tenant.Phone,
                    Address = tenant.Address,
                    CurrentPlan = tenant.CurrentPlan.ToString(),
                    PlanExpiryDate = tenant.PlanExpiryDate,
                    IsActive = tenant.IsActive,
                    CreatedAt = tenant.CreatedAt
                };

                return ApiResponse<TenantDto>.Success(tenantDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<TenantDto>.Error($"Error retrieving tenant: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResult<TenantDto>>> GetTenantsAsync(int page = 1, int pageSize = 10, string searchTerm = "")
        {
            try
            {
                var tenants = await _tenantRepository.GetAllAsync(page, pageSize, searchTerm);
                
                var tenantDtos = tenants.Items.Select(t => new TenantDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Subdomain = t.Subdomain,
                    Email = t.Email,
                    Phone = t.Phone,
                    Address = t.Address,
                    CurrentPlan = t.CurrentPlan.ToString(),
                    PlanExpiryDate = t.PlanExpiryDate,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt
                }).ToList();

                var totalPages = (int)Math.Ceiling((double)tenants.TotalCount / pageSize);
                var result = new PagedResult<TenantDto>
                {
                    Items = tenantDtos,
                    TotalCount = tenants.TotalCount,
                    Page = page,
                    PageSize = pageSize
                };

                return ApiResponse<PagedResult<TenantDto>>.Success(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResult<TenantDto>>.Error($"Error retrieving tenants: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TenantDto>> UpdateTenantAsync(int tenantId, UpdateTenantDto updateTenantDto)
        {
            try
            {
                var tenant = await _tenantRepository.GetByIdAsync(tenantId);
                if (tenant == null)
                {
                    return ApiResponse<TenantDto>.Error("Tenant not found");
                }

                // Check if subdomain is being changed and if it already exists
                if (updateTenantDto.Subdomain != null && updateTenantDto.Subdomain != tenant.Subdomain)
                {
                    var existingTenant = await _tenantRepository.GetBySubdomainAsync(updateTenantDto.Subdomain);
                    if (existingTenant != null)
                    {
                        return ApiResponse<TenantDto>.Error("Subdomain already exists");
                    }
                }

                // Update tenant properties
                if (updateTenantDto.Name != null) tenant.Name = updateTenantDto.Name;
                if (updateTenantDto.Subdomain != null) tenant.Subdomain = updateTenantDto.Subdomain;
                if (updateTenantDto.PhoneNumber != null) tenant.Phone = updateTenantDto.PhoneNumber;
                if (updateTenantDto.Address != null) tenant.Address = updateTenantDto.Address;

                var updatedTenant = await _tenantRepository.UpdateAsync(tenant);
                if (updatedTenant == null)
                {
                    return ApiResponse<TenantDto>.Error("Failed to update tenant");
                }

                var tenantDto = new TenantDto
                {
                    Id = updatedTenant.Id,
                    Name = updatedTenant.Name,
                    Subdomain = updatedTenant.Subdomain,
                    Email = updatedTenant.Email,
                    Phone = updatedTenant.Phone,
                    Address = updatedTenant.Address,
                    CurrentPlan = updatedTenant.CurrentPlan.ToString(),
                    PlanExpiryDate = updatedTenant.PlanExpiryDate,
                    IsActive = updatedTenant.IsActive,
                    CreatedAt = updatedTenant.CreatedAt
                };

                return ApiResponse<TenantDto>.Success(tenantDto, "Tenant updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<TenantDto>.Error($"Error updating tenant: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteTenantAsync(int tenantId)
        {
            try
            {
                var tenant = await _tenantRepository.GetByIdAsync(tenantId);
                if (tenant == null)
                {
                    return ApiResponse<bool>.Error("Tenant not found");
                }

                var result = await _tenantRepository.DeleteAsync(tenantId);
                return ApiResponse<bool>.Success(result, "Tenant deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Error deleting tenant: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> UpdateTenantPlanAsync(int tenantId, string newPlan)
        {
            try
            {
                var tenant = await _tenantRepository.GetByIdAsync(tenantId);
                if (tenant == null)
                {
                    return ApiResponse<bool>.Error("Tenant not found");
                }

                // Validate plan
                if (!AppConstants.ServicePlans.AllPlans.Contains(newPlan))
                {
                    return ApiResponse<bool>.Error("Invalid plan");
                }

                // Parse string to enum
                if (Enum.TryParse<ServicePlan>(newPlan, out var planEnum))
                {
                    tenant.CurrentPlan = planEnum;
                }
                else
                {
                    return ApiResponse<bool>.Error("Invalid plan format");
                }
                tenant.PlanExpiryDate = DateTime.UtcNow.AddMonths(1); // Default to 1 month

                var updatedTenant = await _tenantRepository.UpdateAsync(tenant);
                if (updatedTenant == null)
                {
                    return ApiResponse<bool>.Error("Failed to update tenant plan");
                }

                return ApiResponse<bool>.Success(true, "Tenant plan updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Error updating tenant plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TenantStatsDto>> GetTenantStatsAsync(int tenantId)
        {
            // TODO: Implement GetTenantStatsAsync in repository
            var stats = new TenantStatsDto
            {
                TotalEvents = 0,
                TotalGuests = 0
            };
            return await Task.FromResult(ApiResponse<TenantStatsDto>.Success(stats));
        }

        public async Task<ApiResponse<List<TenantDto>>> GetExpiringTenantsAsync(int daysThreshold = 7)
        {
            // TODO: Implement GetExpiringTenantsAsync in repository
            var emptyList = new List<TenantDto>();
            return await Task.FromResult(ApiResponse<List<TenantDto>>.Success(emptyList));
        }

        public async Task<ApiResponse<bool>> ActivateTenantAsync(int tenantId)
        {
            try
            {
                var tenant = await _tenantRepository.GetByIdAsync(tenantId);
                if (tenant == null)
                {
                    return ApiResponse<bool>.Error("Tenant not found");
                }

                tenant.IsActive = true;
                var updatedTenant = await _tenantRepository.UpdateAsync(tenant);
                
                if (updatedTenant == null)
                {
                    return ApiResponse<bool>.Error("Failed to activate tenant");
                }

                return ApiResponse<bool>.Success(true, "Tenant activated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Error activating tenant: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeactivateTenantAsync(int tenantId)
        {
            try
            {
                var tenant = await _tenantRepository.GetByIdAsync(tenantId);
                if (tenant == null)
                {
                    return ApiResponse<bool>.Error("Tenant not found");
                }

                tenant.IsActive = false;
                var updatedTenant = await _tenantRepository.UpdateAsync(tenant);
                
                if (updatedTenant == null)
                {
                    return ApiResponse<bool>.Error("Failed to deactivate tenant");
                }

                return ApiResponse<bool>.Success(true, "Tenant deactivated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Error deactivating tenant: {ex.Message}");
            }
        }
    }
} 