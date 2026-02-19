using FluentValidation;
using Roadmap.Domain.Entities;

namespace Roadmap.Application.DTOs;

public record RegisterRequestDto(string Email, string DisplayName, string Password);
public record LoginRequestDto(string Email, string Password);
public record AuthResponseDto(string AccessToken, string RefreshToken, string Email, string Role);

public class FeatureQueryDto
{
    public string? Search { get; set; }
    public FeatureStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public record CreateFeatureDto(string Title, string Description);
public record UpdateStatusDto(FeatureStatus Status);

public class FeatureResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FeatureStatus Status { get; set; }
    public int VoteCount { get; set; }
    public IReadOnlyCollection<CommentResponseDto> Comments { get; set; } = [];
}

public record CreateCommentDto(string Body);
public class CommentResponseDto
{
    public Guid Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class RegisterValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.DisplayName).MinimumLength(2);
        RuleFor(x => x.Password).MinimumLength(8);
    }
}

public class CreateFeatureValidator : AbstractValidator<CreateFeatureDto>
{
    public CreateFeatureValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
    }
}
