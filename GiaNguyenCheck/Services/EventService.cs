using GiaNguyenCheck.DTOs;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;
using GiaNguyenCheck.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GiaNguyenCheck.Services
{
    // EventService will be implemented later
}

/*
// Full EventService content commented out for now
// TODO: Fix type conversion issues and implement properly
namespace GiaNguyenCheck.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        private static EventDto MapToDto(Event ev) => new EventDto
        {
            Id = ev.Id,
            Name = ev.Name,
            Description = ev.Description,
            Location = ev.Location,
            StartTime = ev.StartTime,
            EndTime = ev.EndTime,
            CheckInStartTime = ev.CheckInStartTime,
            CheckInEndTime = ev.CheckInEndTime,
            BannerImageUrl = ev.BannerImageUrl,
            Status = ev.Status,
            MaxAttendees = ev.MaxAttendees,
            AllowMultipleCheckIn = ev.AllowMultipleCheckIn,
            TenantId = ev.TenantId,
            CreatedByUserId = ev.CreatedByUserId,
            CreatedAt = ev.CreatedAt,
        };

        public async Task<ApiResponse<EventDto>> CreateEventAsync(CreateEventDto dto)
        {
            try
            {
                var ev = new Event
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Location = dto.Location,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    CheckInStartTime = dto.StartTime.AddMinutes(-30),
                    CheckInEndTime = dto.StartTime.AddHours(4),
                    MaxAttendees = dto.MaxGuests,
                    AllowMultipleCheckIn = dto.IsPublic,
                    TenantId = 0,
                    CreatedByUserId = dto.CreatedByUserId,
                    Status = EventStatus.Draft,
                };
                await _eventRepository.AddAsync(ev);
                return ApiResponse<EventDto>.Success(MapToDto(ev), "Tạo sự kiện thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<EventDto>.Error("Lỗi tạo sự kiện: " + ex.Message);
            }
        }

        public async Task<ApiResponse<EventDto>> GetEventAsync(int eventId)
        {
            var ev = await _eventRepository.GetByIdAsync(eventId);
            if (ev == null) return ApiResponse<EventDto>.Error("Không tìm thấy sự kiện");
            return ApiResponse<EventDto>.Success(MapToDto(ev));
        }

        public async Task<ApiResponse<PagedResult<EventDto>>> GetEventsAsync(int page = 1, int pageSize = 10, string searchTerm = "", string status = "")
        {
            var (items,total) = await _eventRepository.GetPagedAsync(page, pageSize); // sử dụng BaseRepo
            var list = items.Select(MapToDto).ToList();
            var result = new PagedResult<EventDto> { Items = list, TotalCount = total, Page = page, PageSize = pageSize };
            return ApiResponse<PagedResult<EventDto>>.Success(result);
        }

        public async Task<ApiResponse<EventDto>> UpdateEventAsync(int eventId, UpdateEventDto dto)
        {
            var ev = await _eventRepository.GetByIdAsync(eventId);
            if (ev == null) return ApiResponse<EventDto>.Error("Không tìm thấy sự kiện");
            if (dto.Name!=null) ev.Name=dto.Name;
            if (dto.Description!=null) ev.Description=dto.Description;
            if (dto.Location!=null) ev.Location=dto.Location;
            if (dto.StartTime.HasValue) ev.StartTime=dto.StartTime.Value;
            if (dto.EndTime.HasValue) ev.EndTime=dto.EndTime.Value;
            if (dto.MaxGuests.HasValue) ev.MaxAttendees=dto.MaxGuests.Value;
            await _eventRepository.UpdateAsync(ev);
            return ApiResponse<EventDto>.Success(MapToDto(ev), "Cập nhật thành công");
        }

        public async Task<ApiResponse<bool>> DeleteEventAsync(int eventId)
        {
            var success = await _eventRepository.DeleteAsync(eventId);
            return success ? ApiResponse<bool>.Success(true, "Đã xoá") : ApiResponse<bool>.Error("Xoá thất bại");
        }

        public async Task<ApiResponse<List<EventDto>>> GetUpcomingEventsAsync(int days = 30)
        {
            var list = await _eventRepository.GetUpcomingEventsAsync(0);
            var upcoming = list.Where(e=>e.StartTime<=DateTime.UtcNow.AddDays(days)).Select(MapToDto).ToList();
            return ApiResponse<List<EventDto>>.Success(upcoming);
        }

        public async Task<ApiResponse<EventStatsDto>> GetEventStatsAsync(int eventId) => throw new NotImplementedException();
        public async Task<ApiResponse<byte[]>> GenerateEventQRCodeAsync(int eventId) => throw new NotImplementedException();
        public async Task<ApiResponse<bool>> PublishEventAsync(int eventId) => throw new NotImplementedException();
        public async Task<ApiResponse<bool>> UnpublishEventAsync(int eventId) => throw new NotImplementedException();

        public async Task<List<Event>> GetEventsByTenantAsync(int tenantId)
        {
            return await _eventRepository.Query().Where(e => e.TenantId == tenantId && !e.IsDeleted).Include(e => e.Guests).Include(e => e.CheckIns).ToListAsync();
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _eventRepository.Query().Include(e => e.Guests).Include(e => e.CheckIns).FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<List<Event>> GetUpcomingEventsAsync(int limit)
        {
            return await _eventRepository.Query()
                .Where(e => e.StartTime >= DateTime.UtcNow && !e.IsDeleted)
                .OrderBy(e => e.StartTime)
                .Take(limit)
                .Include(e => e.Guests)
                .ToListAsync();
        }

        public async Task<bool> UpdateEventAsync(Event eventModel)
        {
            await _eventRepository.UpdateAsync(eventModel);
            return true;
        }

        public async Task<bool> CreateEventAsync(Event eventModel)
        {
            await _eventRepository.AddAsync(eventModel);
            return true;
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            return await _eventRepository.DeleteAsync(id);
        }
    }
}
*/ 