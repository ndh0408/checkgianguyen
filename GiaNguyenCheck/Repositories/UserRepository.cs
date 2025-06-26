using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GiaNguyenCheck.Data;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;
using GiaNguyenCheck.DTOs;

namespace GiaNguyenCheck.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserRepository(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _userManager.FindByIdAsync(id.ToString());
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<(IEnumerable<User>, int)> GetPagedAsync(int page, int pageSize)
        {
            var query = _context.Users.AsQueryable();
            var totalCount = await query.CountAsync();
            
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<User> AddAsync(User entity)
        {
            var result = await _userManager.CreateAsync(entity);
            if (result.Succeeded)
                return entity;
            
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task<User> UpdateAsync(User entity)
        {
            var result = await _userManager.UpdateAsync(entity);
            if (result.Succeeded)
                return entity;
            
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
                return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetByTenantIdAsync(int tenantId)
        {
            return await _context.Users
                .Where(u => u.TenantId == tenantId)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            return await _userManager.GetUsersInRoleAsync(role.ToString());
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                return false;

            user.LastLoginAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> SetActiveStatusAsync(int userId, bool isActive)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                return false;

            user.IsActive = isActive;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<DTOs.ApiResponse<User>> CreateAsync(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                return DTOs.ApiResponse<User>.Success(user, "User created");
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            return DTOs.ApiResponse<User>.Error("Failed to create user", errors);
        }

        public async Task<bool> AssignRoleAsync(int userId, string roleName)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }
    }
} 