using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadmap.Application;
using Roadmap.Application.DTOs;

namespace Roadmap.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public Task<AuthResponseDto> Register(RegisterRequestDto dto, CancellationToken ct) => authService.RegisterAsync(dto, ct);

    [HttpPost("login")]
    public Task<AuthResponseDto> Login(LoginRequestDto dto, CancellationToken ct) => authService.LoginAsync(dto, ct);

    [HttpPost("refresh")]
    public Task<AuthResponseDto> Refresh([FromBody] string token, CancellationToken ct) => authService.RefreshAsync(token, ct);
}

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features")]
public class FeaturesController(IFeatureService featureService) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<FeatureResponseDto>> Get([FromQuery] FeatureQueryDto query, CancellationToken ct) => featureService.GetFeaturesAsync(query, ct);

    [Authorize]
    [HttpPost]
    public Task<FeatureResponseDto> Create(CreateFeatureDto dto, CancellationToken ct) => featureService.CreateFeatureAsync(dto, ct);

    [Authorize]
    [HttpPost("{id:guid}/votes")]
    public async Task<IActionResult> Vote(Guid id, CancellationToken ct) { await featureService.UpvoteAsync(id, ct); return NoContent(); }

    [Authorize]
    [HttpPost("{id:guid}/comments")]
    public Task<CommentResponseDto> Comment(Guid id, CreateCommentDto dto, CancellationToken ct) => featureService.AddCommentAsync(id, dto, ct);

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> Status(Guid id, UpdateStatusDto dto, CancellationToken ct) { await featureService.UpdateStatusAsync(id, dto, ct); return NoContent(); }
}

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(IRepository<Roadmap.Domain.Entities.User> users, IRepository<Roadmap.Domain.Entities.Comment> comments, IRepository<Roadmap.Domain.Entities.FeatureRequest> features) : ControllerBase
{
    [HttpGet("analytics")]
    public object Analytics() => new
    {
        users = users.Query().Count(),
        features = features.Query().Count(),
        approvedComments = comments.Query().Count(c => c.IsApproved)
    };

    [HttpPatch("comments/{id:guid}/moderate")]
    public IActionResult Moderate(Guid id, [FromBody] bool approved)
    {
        var comment = comments.Query().SingleOrDefault(x => x.Id == id);
        if (comment is null) return NotFound();
        comment.IsApproved = approved;
        return NoContent();
    }
}
