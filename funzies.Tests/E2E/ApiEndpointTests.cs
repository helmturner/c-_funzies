using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace funzies.Tests.E2E;

public class ApiEndpointTests : E2ETestBase
{
    private readonly HttpClient _client;

    public ApiEndpointTests(PlaywrightFixture fixture) : base(fixture)
    {
        _client = Fixture.ApplicationFactory.CreateClient();
    }

    [Fact]
    public async Task ChatEndpoint_ShouldRespond_WithValidResponse()
    {
        // Arrange
        var requestContent = new StringContent(
            JsonSerializer.Serialize(new { message = "Hello, can you help me?" }), 
            Encoding.UTF8, 
            "application/json");

        // Act
        var response = await _client.PostAsync("/chat", requestContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotBeNull();

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();
        
        // Check if the response is a valid JSON with an expected format
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);
        responseObject.TryGetProperty("response", out _).Should().BeTrue("Response should contain 'response' property");
    }

    [Fact]
    public async Task ChatEndpoint_WithEmptyMessage_ShouldReturnBadRequest()
    {
        // Arrange
        var requestContent = new StringContent(
            JsonSerializer.Serialize(new { message = string.Empty }), 
            Encoding.UTF8, 
            "application/json");

        // Act
        var response = await _client.PostAsync("/chat", requestContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChatEndpoint_WithInvalidPayload_ShouldReturnBadRequest()
    {
        // Arrange
        var requestContent = new StringContent(
            "{ invalid json", 
            Encoding.UTF8, 
            "application/json");

        // Act
        var response = await _client.PostAsync("/chat", requestContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}