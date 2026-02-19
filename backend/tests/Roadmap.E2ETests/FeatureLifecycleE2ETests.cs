using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Roadmap.Application;
using Roadmap.Application.DTOs;

namespace Roadmap.E2ETests;

public class FeatureLifecycleE2ETests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task User_EndToEnd_Journey_ShouldSupport_AuthRefresh_And_FeatureLifecycle()
    {
        var email = $"e2e-{Guid.NewGuid():N}@example.com";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequestDto(email, "E2E Tester", "Password123!"));
        registerResponse.IsSuccessStatusCode.Should().BeTrue();

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        auth.Should().NotBeNull();

        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", auth!.RefreshToken);
        refreshResponse.IsSuccessStatusCode.Should().BeTrue();
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        refreshed.Should().NotBeNull();
        refreshed!.AccessToken.Should().NotBe(auth.AccessToken);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/features",
            new CreateFeatureDto("Dark mode scheduler", "Allow users to schedule dark mode"));

        createResponse.IsSuccessStatusCode.Should().BeTrue();
        var createdFeature = await createResponse.Content.ReadFromJsonAsync<FeatureResponseDto>();
        createdFeature.Should().NotBeNull();

        var voteResponse1 = await _client.PostAsync($"/api/v1/features/{createdFeature!.Id}/votes", null);
        var voteResponse2 = await _client.PostAsync($"/api/v1/features/{createdFeature.Id}/votes", null);
        voteResponse1.IsSuccessStatusCode.Should().BeTrue();
        voteResponse2.IsSuccessStatusCode.Should().BeTrue();

        var commentResponse = await _client.PostAsJsonAsync(
            $"/api/v1/features/{createdFeature.Id}/comments",
            new CreateCommentDto("Need this for late-night workflows"));
        commentResponse.IsSuccessStatusCode.Should().BeTrue();

        var listResponse = await _client.GetFromJsonAsync<PagedResult<FeatureResponseDto>>("/api/v1/features?page=1&pageSize=10");
        listResponse.Should().NotBeNull();

        var fetchedFeature = listResponse!.Items.Single(x => x.Id == createdFeature.Id);
        fetchedFeature.VoteCount.Should().Be(1);
        fetchedFeature.Comments.Should().ContainSingle(c => c.Body == "Need this for late-night workflows");
    }
}
