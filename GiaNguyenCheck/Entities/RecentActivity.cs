using System;

namespace GiaNguyenCheck.Entities
{
    public class RecentActivity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Action { get; set; } = "";
        public string GuestName { get; set; } = "";
        public string EventName { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "";
    }
} 