namespace GiaNguyenCheck.Entities
{
    public class GuestTemplate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Category { get; set; } = "";
    }
} 