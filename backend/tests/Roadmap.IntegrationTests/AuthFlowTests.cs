using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Roadmap.Application.DTOs;

namespace Roadmap.IntegrationTests;

public class AuthFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public AuthFlowTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Register_ShouldReturnToken()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequestDto("test@example.com", "Tester", "Password123!"));
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
