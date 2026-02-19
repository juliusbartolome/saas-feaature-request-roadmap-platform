namespace Roadmap.Domain.Entities;

public enum FeatureStatus { Planned, InProgress, Released }

public class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}

public class FeatureRequest : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FeatureStatus Status { get; set; } = FeatureStatus.Planned;
    public Guid CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    public List<Vote> Votes { get; set; } = [];
    public List<Comment> Comments { get; set; } = [];
}

public class Vote : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid FeatureRequestId { get; set; }
    public FeatureRequest? FeatureRequest { get; set; }
}

public class Comment : BaseEntity
{
    public Guid FeatureRequestId { get; set; }
    public FeatureRequest? FeatureRequest { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = true;
}

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByToken { get; set; }
    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;
}
