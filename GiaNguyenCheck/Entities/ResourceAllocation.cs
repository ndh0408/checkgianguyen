using System.Collections.Generic;

namespace GiaNguyenCheck.Entities
{
    public class ResourceAllocation
    {
        public int AssignedStaff { get; set; }
        public int AvailableStaff { get; set; }
        public List<Event> EventsNeedingStaff { get; set; } = new();
    }
} 