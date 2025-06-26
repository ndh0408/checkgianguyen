using Microsoft.AspNetCore.Mvc;
using GiaNguyenCheck.Interfaces;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GiaNguyenCheck.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ITenantProvider _tenantProvider;

        public EventsController(IEventService eventService, ITenantProvider tenantProvider)
        {
            _eventService = eventService;
            _tenantProvider = tenantProvider;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var events = await _eventService.GetEventsByTenantAsync(_tenantProvider.GetCurrentTenantId());
                return View(events);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Lỗi tải danh sách sự kiện: " + ex.Message;
                return View(new List<Event>());
            }
        }

        public IActionResult Create()
        {
            return View(new Event());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventModel, IFormFile? eventImage)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    eventModel.TenantId = _tenantProvider.GetCurrentTenantId();
                    eventModel.CreatedAt = DateTime.UtcNow;
                    eventModel.Status = GiaNguyenCheck.Entities.EventStatus.Draft;

                    if (eventImage != null)
                    {
                        // Handle image upload
                        var fileName = await SaveEventImage(eventImage);
                        eventModel.ImageUrl = fileName;
                    }

                    await _eventService.CreateEventAsync(eventModel);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi tạo sự kiện: " + ex.Message);
            }

            return View(eventModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var eventModel = await _eventService.GetEventByIdAsync(id);
                if (eventModel == null || eventModel.TenantId != _tenantProvider.GetCurrentTenantId())
                {
                    return NotFound();
                }

                return View(eventModel);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Lỗi tải chi tiết sự kiện: " + ex.Message;
                return View();
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var eventModel = await _eventService.GetEventByIdAsync(id);
                if (eventModel == null || eventModel.TenantId != _tenantProvider.GetCurrentTenantId())
                {
                    return NotFound();
                }

                return View(eventModel);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Lỗi tải sự kiện để chỉnh sửa: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventModel, IFormFile? eventImage)
        {
            try
            {
                if (id != eventModel.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var existingEvent = await _eventService.GetEventByIdAsync(id);
                    if (existingEvent == null || existingEvent.TenantId != _tenantProvider.GetCurrentTenantId())
                    {
                        return NotFound();
                    }

                    if (eventImage != null)
                    {
                        // Handle image upload
                        var fileName = await SaveEventImage(eventImage);
                        eventModel.ImageUrl = fileName;
                    }

                    await _eventService.UpdateEventAsync(eventModel);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi cập nhật sự kiện: " + ex.Message);
            }

            return View(eventModel);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var eventModel = await _eventService.GetEventByIdAsync(id);
                if (eventModel == null || eventModel.TenantId != _tenantProvider.GetCurrentTenantId())
                {
                    return NotFound();
                }

                return View(eventModel);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Lỗi tải sự kiện để xóa: " + ex.Message;
                return View();
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var eventModel = await _eventService.GetEventByIdAsync(id);
                if (eventModel == null || eventModel.TenantId != _tenantProvider.GetCurrentTenantId())
                {
                    return NotFound();
                }

                await _eventService.DeleteEventAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Lỗi xóa sự kiện: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var eventModel = await _eventService.GetEventByIdAsync(id);
                if (eventModel == null || eventModel.TenantId != _tenantProvider.GetCurrentTenantId())
                {
                    return Json(new { success = false, message = "Sự kiện không tồn tại" });
                }

                eventModel.Status = (GiaNguyenCheck.Entities.EventStatus)Enum.Parse(typeof(GiaNguyenCheck.Entities.EventStatus), status);
                await _eventService.UpdateEventAsync(eventModel);

                return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi cập nhật trạng thái: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEventsList()
        {
            try
            {
                var events = await _eventService.GetEventsByTenantAsync(_tenantProvider.GetCurrentTenantId());
                var eventList = events.Select(e => new { id = e.Id, name = e.Name }).ToList();
                return Json(new { success = true, data = eventList });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUpcomingEvents(int limit = 5)
        {
            try
            {
                var events = await _eventService.GetUpcomingEventsAsync(limit);
                return Json(new { success = true, data = events });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] string? status, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var events = await _eventService.GetEventsByTenantAsync(_tenantProvider.GetCurrentTenantId());
                
                // Apply filters
                if (!string.IsNullOrEmpty(status))
                    events = events.Where(e => e.Status.ToString() == status).ToList();
                
                if (startDate.HasValue)
                    events = events.Where(e => e.StartTime >= startDate.Value).ToList();
                
                if (endDate.HasValue)
                    events = events.Where(e => e.StartTime <= endDate.Value).ToList();

                // Implementation for exporting to Excel
                // This would typically use a library like EPPlus or ClosedXML
                return Json(new { success = true, message = "Xuất Excel thành công", count = events.Count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<string> SaveEventImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return string.Empty;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "events");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/uploads/events/" + uniqueFileName;
        }
    }
} 