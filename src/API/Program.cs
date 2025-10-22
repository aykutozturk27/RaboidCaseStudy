using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RaboidCaseStudy.Application.Abstractions.Repositories;
using RaboidCaseStudy.Application.Abstractions.Security;
using RaboidCaseStudy.Infrastructure.Config;
using RaboidCaseStudy.Infrastructure.Persistence;
using RaboidCaseStudy.Infrastructure.Security;
using RaboidCaseStudy.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

// Mongo
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));
builder.Services.AddSingleton<MongoContext>();

// Generic Repository & Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<Seeder>();

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ Swagger + JWT Auth entegrasyonu
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "RaboidCaseStudy API",
		Version = "v1",
		Description = "Raboid Case Study API with JWT Auth (Admin & Client roles)"
	});

	var jwtSecurityScheme = new OpenApiSecurityScheme
	{
		Scheme = "bearer",
		BearerFormat = "JWT",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.Http,
		Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
		Reference = new OpenApiReference
		{
			Id = JwtBearerDefaults.AuthenticationScheme,
			Type = ReferenceType.SecurityScheme
		}
	};

	c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			jwtSecurityScheme, Array.Empty<string>()
		}
	});
});

// ✅ JWT Authentication
var secret = builder.Configuration["Jwt:Secret"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["Jwt:Issuer"],
			ValidAudience = builder.Configuration["Jwt:Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
		};
	});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "RaboidCaseStudy API v1");
		c.RoutePrefix = string.Empty; // Swagger ana sayfada açılsın
	});
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ✅ Seed işlemi (ilk çalıştırmada admin ve roller eklenir)
using (var scope = app.Services.CreateScope())
{
	var seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
	await seeder.SeedAsync();
}

app.Run();
