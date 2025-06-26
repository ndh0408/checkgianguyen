namespace GiaNguyenCheck.Entities
{
    public class TodayStats
    {
        public int TotalCheckins { get; set; }
        public int SuccessfulCheckins { get; set; }
        public int FailedCheckins { get; set; }
        public decimal SuccessRate { get; set; }
        public int EventsWorked { get; set; }
    }
} 