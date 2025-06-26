using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GiaNguyenCheck.Data;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;

namespace GiaNguyenCheck.Repositories;

/// <summary>
/// Repository for User management với Identity integration
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public UserRepository(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .ToListAsync();
    }

    public async Task<(IEnumerable<User>, int)> GetPagedAsync(int page, int pageSize)
    {
        var totalCount = await _context.Users
            .Where(u => !u.IsDeleted)
            .CountAsync();

        var users = await _context.Users
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<User> AddAsync(User entity)
    {
        var result = await _userManager.CreateAsync(entity);
        if (result.Succeeded)
        {
            return entity;
        }
        throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    public async Task<User> UpdateAsync(User entity)
    {
        var result = await _userManager.UpdateAsync(entity);
        if (result.Succeeded)
        {
            return entity;
        }
        throw new InvalidOperationException($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            user.IsDeleted = true;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        return false;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        return user != null && !user.IsDeleted;
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
    {
        return await _context.Users
            .Where(u => u.Role == role && !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<bool> UpdateLastLoginAsync(Guid userId)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        return false;
    }

    public async Task<bool> SetActiveStatusAsync(Guid userId, bool isActive)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        return false;
    }

    public async Task<IEnumerable<User>> GetTenantUsersAsync(Guid tenantId)
    {
        return await _context.Users
            .Where(u => u.TenantId == tenantId && !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<(IEnumerable<User>, int)> SearchUsersAsync(
        string? searchTerm = null,
        UserRole? role = null,
        Guid? tenantId = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _context.Users.Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u =>
                u.FirstName.Contains(searchTerm) ||
                u.LastName.Contains(searchTerm) ||
                u.Email.Contains(searchTerm));
        }

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        if (tenantId.HasValue)
        {
            query = query.Where(u => u.TenantId == tenantId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null)
    {
        var user = await GetByEmailAsync(email);
        
        if (user == null || user.IsDeleted)
            return false;

        if (excludeUserId.HasValue && user.Id == excludeUserId.Value)
            return false;

        return true;
    }

    public async Task<bool> ValidatePasswordAsync(User user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<bool> ChangePasswordAsync(User user, string currentPassword, string newPassword)
    {
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> ResetPasswordAsync(User user, string token, string newPassword)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }
} 