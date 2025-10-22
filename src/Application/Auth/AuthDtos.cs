namespace RaboidCaseStudy.Application.Auth;
public record RegisterRequest(string Email, string Password, IEnumerable<string>? Roles);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string AccessToken, string UserId, string Email, IEnumerable<string> Roles);
