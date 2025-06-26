using System.Collections.Generic;

namespace GiaNguyenCheck.Entities
{
    public class Plan
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public string BillingCycle { get; set; } = "Monthly";
        public int MaxEvents { get; set; }
        public int MaxGuests { get; set; }
        public int MaxUsers { get; set; }
        public string Features { get; set; } = "";
        public PlanType PlanType { get; set; } = PlanType.Pro;
    }
} 