using System;

namespace GiaNguyenCheck.Entities
{
    public class SearchHistory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SearchTerm { get; set; } = "";
        public DateTime SearchDate { get; set; } = DateTime.UtcNow;
        public int ResultsCount { get; set; }
    }
} 