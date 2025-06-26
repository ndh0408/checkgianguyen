namespace GiaNguyenCheck.Entities
{
    public class EmailTemplate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Content { get; set; } = "";
    }
} 