using RaboidCaseStudy.Domain.Common;
namespace RaboidCaseStudy.Domain.Users;
public class Role : Entity
{
    public string Name { get; set; } = default!; // e.g., "Admin", "Client"
}
