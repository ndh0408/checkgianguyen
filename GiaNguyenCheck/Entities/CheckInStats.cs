using System;
using System.Collections.Generic;

namespace GiaNguyenCheck.Entities
{
    public class CheckInStats
    {
        public int TotalGuests { get; set; }
        public int CheckedInGuests { get; set; }
        public decimal CheckInRate { get; set; }
        public List<string> TimeSlots { get; set; } = new();
        public List<int> CheckinCounts { get; set; } = new();
    }
} 