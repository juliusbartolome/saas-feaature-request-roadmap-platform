using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Roadmap.Application;
using Roadmap.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Roadmap.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<FeatureRequest> Features => Set<FeatureRequest>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Vote>().HasIndex(x => new { x.FeatureRequestId, x.UserId }).IsUnique();
        modelBuilder.Entity<Comment>().Property(x => x.Body).HasMaxLength(1500);
    }
}

public class EfRepository<T>(AppDbContext db) : IRepository<T> where T : BaseEntity
{
    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) => db.Set<T>().SingleOrDefaultAsync(x => x.Id == id, ct);
    public IQueryable<T> Query() => db.Set<T>();
    public Task AddAsync(T entity, CancellationToken ct = default) => db.Set<T>().AddAsync(entity, ct).AsTask();
    public void Remove(T entity) => db.Set<T>().Remove(entity);
}

public class JwtTokenService(IConfiguration config) : IJwtTokenService
{
    public string CreateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.Role) };
        var token = new JwtSecurityToken(expires: DateTime.UtcNow.AddMinutes(20), claims: claims, signingCredentials: creds, issuer: config["Jwt:Issuer"], audience: config["Jwt:Audience"]);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public string CreateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}

public class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public CurrentUser? User
    {
        get
        {
            var claims = httpContextAccessor.HttpContext?.User;
            if (claims?.Identity?.IsAuthenticated != true) return null;
            var idClaim = claims.FindFirst(JwtRegisteredClaimNames.Sub);
            var emailClaim = claims.FindFirst(ClaimTypes.Email);
            var roleClaim = claims.FindFirst(ClaimTypes.Role);


            return idClaim?.Value is null || emailClaim?.Value is null || roleClaim?.Value is null
                ? null
                : new CurrentUser(Guid.Parse(idClaim.Value), roleClaim.Value, emailClaim.Value);
        }
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        return services;
    }
}
