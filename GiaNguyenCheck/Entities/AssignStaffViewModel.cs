using System.Collections.Generic;

namespace GiaNguyenCheck.Entities
{
    public class AssignStaffViewModel
    {
        public string EventId { get; set; } = "";
        public List<string> StaffIds { get; set; } = new();
        public string Role { get; set; } = "";
    }
} 