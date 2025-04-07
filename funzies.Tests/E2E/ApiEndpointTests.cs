using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace funzies.Tests.E2E;

public class ApiEndpointTests : E2ETestBase
{
    private readonly HttpClient _client;

    public ApiEndpointTests(PlaywrightFixture fixture)
        : base(fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task ChatEndpoint_ShouldRespond_WithValidResponse()
    {
        // Arrange
        var requestContent = new { message = "Hello, can you help me?" };
        // Act
        var response = await _client.PostAsJsonAsync("/chat", requestContent);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        // Assert
        response
            .IsSuccessStatusCode.Should()
            .BeTrue($"Expected success status code but got {response.StatusCode}");

        responseObject
            .TryGetProperty("text", out _)
            .Should()
            .BeTrue("Response should contain 'text' property");
    }

    [Fact]
    public async Task ChatEndpoint_WithEmptyMessage_ShouldReturnErrorResponse()
    {
        // Arrange
        var emptyMessage = new { message = string.Empty };
        // Act
        var response = await _client.PostAsJsonAsync("/chat", emptyMessage);

        // Assert
        response
            .StatusCode.Should()
            .Be(
                HttpStatusCode.BadRequest,
                $"Expected 400 status code but got {response.StatusCode}"
            );
    }

    [Fact]
    public async Task ChatEndpoint_WithInvalidPayload_ShouldReturnErrorResponse()
    {
        // Arrange
        var requestContent = new StringContent("{ invalid json", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/chat", requestContent);

        // Assert
        response
            .StatusCode.Should()
            .Be(
                HttpStatusCode.BadRequest,
                $"Expected 400 status code but got {response.StatusCode}"
            );
    }
}
