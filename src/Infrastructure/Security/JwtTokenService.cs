using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RaboidCaseStudy.Application.Abstractions.Security;

namespace RaboidCaseStudy.Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
	private readonly IConfiguration _configuration;
	public JwtTokenService(IConfiguration configuration) => _configuration = configuration;

	public string CreateToken(string userId, string email, IEnumerable<string> roles, TimeSpan? lifetime = null)
	{
		var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret missing");
		var issuer = _configuration["Jwt:Issuer"] ?? "RaboidCaseStudy";
		var audience = _configuration["Jwt:Audience"] ?? "RaboidCaseStudyClients";
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		// Temel kimlik bilgileri
		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Sub, userId),
			new(JwtRegisteredClaimNames.Email, email),
			new(ClaimTypes.NameIdentifier, userId),
			new(ClaimTypes.Name, email)
		};

		// Roller hem kýsa hem uzun formda eklenir
		foreach (var role in roles.Distinct())
		{
			claims.Add(new Claim(ClaimTypes.Role, role)); // ASP.NET Core için
			claims.Add(new Claim("role", role));          // Swagger, Angular, Postman için
		}

		var token = new JwtSecurityToken(
			issuer: issuer,
			audience: audience,
			claims: claims,
			expires: DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(8)),
			signingCredentials: creds
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}