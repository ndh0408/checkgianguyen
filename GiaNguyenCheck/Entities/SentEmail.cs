using System;

namespace GiaNguyenCheck.Entities
{
    public class SentEmail
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Subject { get; set; } = "";
        public string Recipient { get; set; } = "";
        public DateTime SentDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "";
    }
} 