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
    private readonly IRepository<User> _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly MongoContext _context;

    public AuthController(IRepository<User> userRepository, 
                          IJwtTokenService jwtTokenService, 
                          MongoContext context)
    {
        _userRepository = userRepository; 
        _jwtTokenService = jwtTokenService; 
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancelationToken)
    {
        var usersCollection = _context.GetCollection<User>();
        if (await usersCollection.Find(u => u.Email == request.Email).AnyAsync(cancelationToken))
            return Conflict("Email already used");

        var (hash, salt) = PasswordHasher.HashPassword(request.Password);
        var user = await _userRepository.InsertAsync(new User {
            Email = request.Email,
            PasswordHash = hash,
            Salt = salt,
            Roles = request.Roles?.ToList() ?? new() { "Client" }
        }, cancelationToken);

        var token = _jwtTokenService.CreateToken(user.Id, user.Email, user.Roles);
        return new AuthResponse(token, user.Id, user.Email, user.Roles);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancelationToken)
    {
        var usersCollection = _context.GetCollection<User>();
        var user = await usersCollection.Find(u => u.Email == request.Email).FirstOrDefaultAsync(cancelationToken);

        if (user is null) 
            return Unauthorized("Invalid credentials");

        if (!PasswordHasher.Verify(request.Password, user.PasswordHash, user.Salt)) 
            return Unauthorized("Invalid credentials");

        var token = _jwtTokenService.CreateToken(user.Id, user.Email, user.Roles);
        return new AuthResponse(token, user.Id, user.Email, user.Roles);
    }
}
