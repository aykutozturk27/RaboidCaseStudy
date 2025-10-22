using RaboidCaseStudy.Domain.Common;
namespace RaboidCaseStudy.Domain.Users;
public class User : Entity
{
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!; // PBKDF2
    public string Salt { get; set; } = default!;
    public List<string> Roles { get; set; } = new();
    public bool IsActive { get; set; } = true;
}
