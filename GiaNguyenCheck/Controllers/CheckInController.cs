/*
using Microsoft.AspNetCore.Mvc;
using GiaNguyenCheck.DTOs;
using GiaNguyenCheck.Interfaces;
using Microsoft.AspNetCore.SignalR;
using GiaNguyenCheck.Hubs;

namespace GiaNguyenCheck.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckInController : ControllerBase
    {
        private readonly ICheckInService _checkInService;
        private readonly IHubContext<DashboardHub> _hubContext;
        public CheckInController(ICheckInService checkInService, IHubContext<DashboardHub> hubContext)
        {
            _checkInService = checkInService;
            _hubContext = hubContext;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanQRCode([FromBody] PerformCheckInDto dto)
        {
            var result = await _checkInService.CheckInGuestAsync(dto.QRCode, 0); // 0: lấy từ JWT thực tế
            if (result.IsSuccess)
            {
                await _hubContext.Clients.Group($"event_{result.Data?.EventId}").SendAsync("ReceiveCheckIn", result.Data);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCheckIns([FromQuery] int eventId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _checkInService.GetCheckInsAsync(eventId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetCheckInStats([FromQuery] int eventId = 0, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var result = await _checkInService.GetCheckInStatsAsync(eventId, fromDate, toDate);
            return Ok(result);
        }

        // TODO: Implement these methods later
        /*
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto)
        {
            var result = await _checkInService.CheckInAsync(dto);
            return Ok(result);
        }

        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut([FromBody] CheckOutDto dto)
        {
            var result = await _checkInService.CheckOutAsync(dto);
            return Ok(result);
        }

        [HttpGet("verify-qr")]
        public async Task<IActionResult> VerifyQRCode([FromQuery] string qrCode)
        {
            var result = await _checkInService.VerifyQRCodeAsync(qrCode);
            return Ok(result);
        }

        [HttpGet("by-event/{eventId}")]
        public async Task<IActionResult> GetCheckInsByEvent(int eventId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _checkInService.GetCheckInsByEventAsync(eventId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("by-guest/{guestId}")]
        public async Task<IActionResult> GetCheckInsByGuest(int guestId)
        {
            var result = await _checkInService.GetCheckInsByGuestAsync(guestId);
            return Ok(result);
        }

        [HttpPost("bulk-check-in")]
        public async Task<IActionResult> BulkCheckIn([FromBody] BulkCheckInDto dto)
        {
            var result = await _checkInService.BulkCheckInAsync(dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCheckIn(int id)
        {
            var result = await _checkInService.DeleteCheckInAsync(id);
            return Ok(result);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportCheckIns([FromQuery] int eventId = 0, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var result = await _checkInService.ExportCheckInsAsync(eventId, fromDate, toDate);
            return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "checkins.xlsx");
        }

        [HttpGet("realtime-stats")]
        public async Task<IActionResult> GetRealtimeStats([FromQuery] int eventId = 0)
        {
            var result = await _checkInService.GetRealtimeStatsAsync(eventId);
            return Ok(result);
        }
        *//*
    }
}
*/ 