using System;

namespace GiaNguyenCheck.Entities
{
    public class ImportHistory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FileName { get; set; } = "";
        public DateTime ImportDate { get; set; } = DateTime.UtcNow;
        public int ImportedCount { get; set; }
        public string Status { get; set; } = "";
    }
} 