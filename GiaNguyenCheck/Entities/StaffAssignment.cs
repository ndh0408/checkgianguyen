using System;

namespace GiaNguyenCheck.Entities
{
    public class StaffAssignment
    {
        public string EventId { get; set; } = "";
        public string EventName { get; set; } = "";
        public string StaffId { get; set; } = "";
        public string StaffName { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime AssignmentDate { get; set; } = DateTime.UtcNow;
    }
} 