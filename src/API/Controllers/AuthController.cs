using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using RaboidCaseStudy.Application.Abstractions.Repositories;
using RaboidCaseStudy.Application.Abstractions.Security;
using RaboidCaseStudy.Application.Auth;
using RaboidCaseStudy.Domain.Users;
using RaboidCaseStudy.Infrastructure.Persistence;
using RaboidCaseStudy.Infrastructure.Security;

namespace RaboidCaseStudy.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IRepository<User> _users;
    private readonly IJwtTokenService _jwt;
    private readonly MongoContext _ctx;
    public AuthController(IRepository<User> users, IJwtTokenService jwt, MongoContext ctx)
    {
        _users = users; _jwt = jwt; _ctx = ctx;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req, CancellationToken ct)
    {
        var usersCol = _ctx.GetCollection<User>();
        if (await usersCol.Find(u => u.Email == req.Email).AnyAsync(ct))
            return Conflict("Email already used");

        var (hash, salt) = PasswordHasher.HashPassword(req.Password);
        var user = await _users.InsertAsync(new User {
            Email = req.Email,
            PasswordHash = hash,
            Salt = salt,
            Roles = req.Roles?.ToList() ?? new() { "Client" }
        }, ct);

        var token = _jwt.CreateToken(user.Id, user.Email, user.Roles);
        return new AuthResponse(token, user.Id, user.Email, user.Roles);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req, CancellationToken ct)
    {
        var usersCol = _ctx.GetCollection<User>();
        var user = await usersCol.Find(u => u.Email == req.Email).FirstOrDefaultAsync(ct);
        if (user is null) return Unauthorized("Invalid credentials");
        if (!PasswordHasher.Verify(req.Password, user.PasswordHash, user.Salt)) return Unauthorized("Invalid credentials");

        var token = _jwt.CreateToken(user.Id, user.Email, user.Roles);
        return new AuthResponse(token, user.Id, user.Email, user.Roles);
    }
}
