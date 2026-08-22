using SaaS.Models;

namespace SaaS.Api.Models
{
    public class Template : BaseModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        // Markdown/HTML/template content
        public string Content { get; set; } = null!;

        public bool IsPublic { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
