namespace SaaS.Api.Models
{
    public class Role : BaseModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
