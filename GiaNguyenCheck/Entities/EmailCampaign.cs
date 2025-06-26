using System;

namespace GiaNguyenCheck.Entities
{
    public class EmailCampaign
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int RecipientCount { get; set; }
    }
} 