using System.Collections.Generic;

namespace GiaNguyenCheck.Entities
{
    public class EventAnalytics
    {
        public List<string> EventNames { get; set; } = new();
        public List<int> GuestCounts { get; set; } = new();
        public List<int> CheckinCounts { get; set; } = new();
    }
} 