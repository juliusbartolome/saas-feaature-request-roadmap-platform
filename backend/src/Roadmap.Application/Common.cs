using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Roadmap.Application.DTOs;
using Roadmap.Domain.Entities;

namespace Roadmap.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IFeatureService, FeatureService>();
        return services;
    }
}

public interface IUnitOfWork { Task<int> SaveChangesAsync(CancellationToken cancellationToken = default); }
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    IQueryable<T> Query();
    Task AddAsync(T entity, CancellationToken ct = default);
    void Remove(T entity);
}

public interface IJwtTokenService
{
    string CreateAccessToken(User user);
    string CreateRefreshToken();
}

public record CurrentUser(Guid UserId, string Role, string Email);
public interface ICurrentUserAccessor { CurrentUser? User { get; } }

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshAsync(string refreshToken, CancellationToken ct = default);
}

public interface IFeatureService
{
    Task<PagedResult<FeatureResponseDto>> GetFeaturesAsync(FeatureQueryDto query, CancellationToken ct = default);
    Task<FeatureResponseDto> CreateFeatureAsync(CreateFeatureDto dto, CancellationToken ct = default);
    Task UpvoteAsync(Guid featureId, CancellationToken ct = default);
    Task<CommentResponseDto> AddCommentAsync(Guid featureId, CreateCommentDto dto, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid featureId, UpdateStatusDto dto, CancellationToken ct = default);
}

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<FeatureRequest, FeatureResponseDto>()
            .ForMember(dest => dest.VoteCount, opt => opt.MapFrom(src => src.Votes.Count))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments.Where(c => c.IsApproved)));
        CreateMap<Comment, CommentResponseDto>();
    }
}

public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = [];
    public int Total { get; set; }
}
