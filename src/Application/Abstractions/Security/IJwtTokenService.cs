namespace RaboidCaseStudy.Application.Abstractions.Security;
public interface IJwtTokenService
{
    string CreateToken(string userId, string email, IEnumerable<string> roles, TimeSpan? lifetime = null);
}
