namespace GiaNguyenCheck.Entities
{
    public class Permission
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Roles { get; set; } = new();
    }
} 