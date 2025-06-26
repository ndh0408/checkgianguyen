/*
using Microsoft.AspNetCore.Mvc;
using GiaNguyenCheck.DTOs;
using GiaNguyenCheck.Interfaces;

namespace GiaNguyenCheck.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuestsController : ControllerBase
    {
        private readonly IGuestService _guestService;
        public GuestsController(IGuestService guestService)
        {
            _guestService = guestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetGuests([FromQuery] int eventId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _guestService.GetGuestsAsync(eventId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGuest(int id)
        {
            var result = await _guestService.GetGuestAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGuest([FromBody] CreateGuestDto dto)
        {
            var result = await _guestService.CreateGuestAsync(dto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGuest(int id, [FromBody] UpdateGuestDto dto)
        {
            var result = await _guestService.UpdateGuestAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGuest(int id)
        {
            var result = await _guestService.DeleteGuestAsync(id);
            return Ok(result);
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportGuests([FromQuery] int eventId, IFormFile file)
        {
            // TODO: Parse file to list of CreateGuestDto
            var guests = new List<CreateGuestDto>();
            var result = await _guestService.ImportGuestsAsync(eventId, guests);
            return Ok(result);
        }

        [HttpGet("export/{eventId}")]
        public async Task<IActionResult> ExportGuests(int eventId)
        {
            // TODO: Implement export service
            var fakeData = new byte[0];
            return File(fakeData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "guests.xlsx");
        }

        [HttpGet("{id}/qr-code")]
        public async Task<IActionResult> GenerateGuestQRCode(int id)
        {
            var result = await _guestService.GenerateGuestQRCodeAsync(id);
            return Ok(result);
        }

        // TODO: Implement these methods later
        /*
        [HttpPost("bulk-send-invitations")]
        public async Task<IActionResult> BulkSendInvitations([FromBody] BulkSendInvitationsDto dto)
        {
            var result = await _guestService.BulkSendInvitationsAsync(dto);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchGuests([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _guestService.SearchGuestsAsync(query, page, pageSize);
            return Ok(result);
        }

        [HttpGet("by-event/{eventId}")]
        public async Task<IActionResult> GetGuestsByEvent(int eventId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _guestService.GetGuestsByEventAsync(eventId, page, pageSize);
            return Ok(result);
        }

        [HttpPost("bulk-update")]
        public async Task<IActionResult> BulkUpdateGuests([FromBody] BulkUpdateGuestsDto dto)
        {
            var result = await _guestService.BulkUpdateGuestsAsync(dto);
            return Ok(result);
        }
        *//*
    }
}
*/ 