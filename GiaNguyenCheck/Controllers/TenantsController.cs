using Microsoft.AspNetCore.Mvc;
using GiaNguyenCheck.DTOs;
using GiaNguyenCheck.Interfaces;

namespace GiaNguyenCheck.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        public TenantsController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTenants([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _tenantService.GetTenantsAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTenant(int id)
        {
            var result = await _tenantService.GetTenantAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDto dto)
        {
            var result = await _tenantService.CreateTenantAsync(dto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTenant(int id, [FromBody] UpdateTenantDto dto)
        {
            var result = await _tenantService.UpdateTenantAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTenant(int id)
        {
            var result = await _tenantService.DeleteTenantAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/update-plan")]
        public async Task<IActionResult> UpdateTenantPlan(int id, [FromBody] string newPlan)
        {
            var result = await _tenantService.UpdateTenantPlanAsync(id, newPlan);
            return Ok(result);
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetTenantStats(int id)
        {
            var result = await _tenantService.GetTenantStatsAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateTenant(int id)
        {
            var result = await _tenantService.ActivateTenantAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateTenant(int id)
        {
            var result = await _tenantService.DeactivateTenantAsync(id);
            return Ok(result);
        }

        [HttpGet("expiring")]
        public async Task<IActionResult> GetExpiringTenants([FromQuery] int daysThreshold = 7)
        {
            var result = await _tenantService.GetExpiringTenantsAsync(daysThreshold);
            return Ok(result);
        }
    }
} 