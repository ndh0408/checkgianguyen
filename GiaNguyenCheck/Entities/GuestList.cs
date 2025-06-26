namespace GiaNguyenCheck.Entities
{
    public class GuestList
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public int GuestCount { get; set; }
        public string EventName { get; set; } = "";
    }
} 