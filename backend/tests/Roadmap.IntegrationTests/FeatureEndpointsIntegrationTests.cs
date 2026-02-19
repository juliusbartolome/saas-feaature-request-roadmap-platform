using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Roadmap.Application;
using Roadmap.Application.DTOs;
using Xunit;

namespace Roadmap.IntegrationTests;

public class FeatureEndpointsIntegrationTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateFeature_ShouldReturnUnauthorized_WhenNoToken()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/features",
            new CreateFeatureDto("No auth feature", "Should fail without JWT"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateFeature_And_GetFeatures_ShouldWork_ForAuthenticatedUser()
    {
        var auth = await RegisterUserAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/features",
            new CreateFeatureDto("Export roadmap", "Add CSV export support"));

        createResponse.IsSuccessStatusCode.Should().BeTrue();

        var list = await _client.GetFromJsonAsync<PagedResult<FeatureResponseDto>>("/api/v1/features?page=1&pageSize=10");
        list.Should().NotBeNull();
        list!.Items.Should().Contain(x => x.Title == "Export roadmap");
    }

    private async Task<AuthResponseDto> RegisterUserAsync()
    {
        var email = $"integration-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequestDto(email, "Integration User", "Password123!"));

        response.IsSuccessStatusCode.Should().BeTrue();
        var payload = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        payload.Should().NotBeNull();
        return payload!;
    }
}
