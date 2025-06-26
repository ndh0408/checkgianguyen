using System;

namespace GiaNguyenCheck.Entities
{
    public class EventDetail
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; } = "";
        public int TotalGuests { get; set; }
        public int CheckedInGuests { get; set; }
        public decimal CheckInRate { get; set; }
    }
} 