namespace GiaNguyenCheck.Entities
{
    public class SMSTemplate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Content { get; set; } = "";
    }
} 