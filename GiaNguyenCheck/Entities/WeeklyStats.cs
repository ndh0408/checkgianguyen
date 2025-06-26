using System.Collections.Generic;

namespace GiaNguyenCheck.Entities
{
    public class WeeklyStats
    {
        public List<string> Days { get; set; } = new();
        public List<int> CheckinCounts { get; set; } = new();
        public List<decimal> SuccessRates { get; set; } = new();
    }
} 