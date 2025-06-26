using System.Collections.Generic;

namespace GiaNguyenCheck.Entities
{
    public class EventTrends
    {
        public List<string> Months { get; set; } = new();
        public List<int> EventCounts { get; set; } = new();
        public List<int> GuestCounts { get; set; } = new();
    }
} 