namespace SaaS.Api.Models
{
    //Goi dich vu
    public class Plan
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string Currency { get; set; } = "VND";

        public int MaxReportsPerMonth { get; set; }
        public long MaxAiTokensPerMonth { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
