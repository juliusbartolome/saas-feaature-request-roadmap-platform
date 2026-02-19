using System.Net.Http.Json;
using FluentAssertions;
using Roadmap.Application.DTOs;

namespace Roadmap.IntegrationTests;

public class AuthFlowTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_And_Login_ShouldReturnTokens()
    {
        var email = $"test-{Guid.NewGuid():N}@example.com";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequestDto(email, "Tester", "Password123!"));

        registerResponse.IsSuccessStatusCode.Should().BeTrue();
        var registerPayload = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        registerPayload.Should().NotBeNull();
        registerPayload!.AccessToken.Should().NotBeNullOrWhiteSpace();
        registerPayload.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequestDto(email, "Password123!"));

        loginResponse.IsSuccessStatusCode.Should().BeTrue();
        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        loginPayload.Should().NotBeNull();
        loginPayload!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }
}
