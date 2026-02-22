using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Roadmap.Application.DTOs;
using Roadmap.Domain.Entities;

namespace Roadmap.Application;

public class AuthService(IRepository<User> users, IRepository<RefreshToken> refreshTokens, IUnitOfWork uow, IJwtTokenService jwt, IPasswordHasher hasher, IValidator<RegisterRequestDto> registerValidator) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default)
    {
        await registerValidator.ValidateAndThrowAsync(dto, ct);
        if (await users.Query().AnyAsync(x => x.Email == dto.Email, ct)) throw new InvalidOperationException("Email already used.");
        var user = new User { Email = dto.Email.ToLowerInvariant(), DisplayName = dto.DisplayName, PasswordHash = hasher.Hash(dto.Password), Role = "User" };
        await users.AddAsync(user, ct);
        var refresh = new RefreshToken { User = user, Token = jwt.CreateRefreshToken(), ExpiresAtUtc = DateTime.UtcNow.AddDays(30) };
        await refreshTokens.AddAsync(refresh, ct);
        await uow.SaveChangesAsync(ct);
        return new(jwt.CreateAccessToken(user), refresh.Token, user.Email, user.Role);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var user = await users.Query().SingleOrDefaultAsync(x => x.Email == dto.Email.ToLowerInvariant(), ct) ?? throw new UnauthorizedAccessException("Invalid credentials");
        if (!hasher.Verify(dto.Password, user.PasswordHash)) throw new UnauthorizedAccessException("Invalid credentials");
        var refresh = new RefreshToken { UserId = user.Id, Token = jwt.CreateRefreshToken(), ExpiresAtUtc = DateTime.UtcNow.AddDays(30) };
        await refreshTokens.AddAsync(refresh, ct);
        await uow.SaveChangesAsync(ct);
        return new(jwt.CreateAccessToken(user), refresh.Token, user.Email, user.Role);
    }

    public async Task<AuthResponseDto> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var token = await refreshTokens.Query().Include(x => x.User).SingleOrDefaultAsync(x => x.Token == refreshToken, ct) ?? throw new UnauthorizedAccessException("Invalid refresh token");
        if (!token.IsActive || token.User is null) throw new UnauthorizedAccessException("Expired refresh token");
        token.RevokedAtUtc = DateTime.UtcNow;
        var newToken = jwt.CreateRefreshToken();
        token.ReplacedByToken = newToken;
        await refreshTokens.AddAsync(new RefreshToken { UserId = token.UserId, Token = newToken, ExpiresAtUtc = DateTime.UtcNow.AddDays(30) }, ct);
        await uow.SaveChangesAsync(ct);
        return new(jwt.CreateAccessToken(token.User), newToken, token.User.Email, token.User.Role);
    }
}

public class FeatureService(IRepository<FeatureRequest> features, IRepository<Vote> votes, IRepository<Comment> comments, IUnitOfWork uow, ICurrentUserAccessor currentUser, IMapper mapper, IValidator<CreateFeatureDto> createFeatureValidator) : IFeatureService
{
    public async Task<PagedResult<FeatureResponseDto>> GetFeaturesAsync(FeatureQueryDto query, CancellationToken ct = default)
    {
        var q = features.Query().AsNoTracking().Include(x => x.Votes).Include(x => x.Comments.Where(c => c.IsApproved));
        if (!string.IsNullOrWhiteSpace(query.Search)) q = q.Where(x => x.Title.Contains(query.Search));
        if (query.Status.HasValue) q = q.Where(x => x.Status == query.Status.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(x => x.Votes.Count)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectTo<FeatureResponseDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
        return new() { Total = total, Items = items };
    }

    public async Task<FeatureResponseDto> CreateFeatureAsync(CreateFeatureDto dto, CancellationToken ct = default)
    {
        await createFeatureValidator.ValidateAndThrowAsync(dto, ct);
        var user = currentUser.User ?? throw new UnauthorizedAccessException();
        var entity = new FeatureRequest { Title = dto.Title, Description = dto.Description, CreatedById = user.UserId };
        await features.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return mapper.Map<FeatureResponseDto>(entity);
    }

    public async Task UpvoteAsync(Guid featureId, CancellationToken ct = default)
    {
        var user = currentUser.User ?? throw new UnauthorizedAccessException();
        var exists = await votes.Query().AnyAsync(v => v.FeatureRequestId == featureId && v.UserId == user.UserId, ct);
        if (!exists) await votes.AddAsync(new Vote { FeatureRequestId = featureId, UserId = user.UserId }, ct);
        await uow.SaveChangesAsync(ct);
    }

    public async Task<CommentResponseDto> AddCommentAsync(Guid featureId, CreateCommentDto dto, CancellationToken ct = default)
    {
        var user = currentUser.User ?? throw new UnauthorizedAccessException();
        var comment = new Comment { FeatureRequestId = featureId, UserId = user.UserId, Body = dto.Body };
        await comments.AddAsync(comment, ct);
        await uow.SaveChangesAsync(ct);
        return mapper.Map<CommentResponseDto>(comment);
    }

    public async Task UpdateStatusAsync(Guid featureId, UpdateStatusDto dto, CancellationToken ct = default)
    {
        var feature = await features.GetByIdAsync(featureId, ct) ?? throw new KeyNotFoundException("Feature not found");
        feature.Status = dto.Status;
        feature.UpdatedAtUtc = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);
    }
}
