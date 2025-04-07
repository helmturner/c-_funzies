using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit.Abstractions;

namespace funzies.Tests.E2E
{
    public class ChatApiTests(PlaywrightFixture fixture, ITestOutputHelper output)
        : PlaywrightTestBase(fixture)
    {
        private readonly ITestOutputHelper _output = output;

        [Fact]
        public async Task ChatEndpoint_ShouldReturnResponse_WhenCalledWithValidMessage()
        {
            // Use HttpClient instead of Playwright's API request
            var httpClient = Fixture.Client;

            // Act
            var response = await httpClient.GetAsync("chat?message=Hello");
            var statusCode = (int)response.StatusCode;
            var content = await response.Content.ReadFromJsonAsync<ChatResponse>();

            // Log for debugging
            _output.WriteLine($"Status: {statusCode} {response.StatusCode}");
            _output.WriteLine($"Content: {JsonSerializer.Serialize(content)}");

            // Assert
            statusCode.Should().Be(200);
            content.Should().NotBeNull();
            content!.Text.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ChatEndpoint_ShouldReturnBadRequest_WhenCalledWithEmptyMessage()
        {
            // Use HttpClient instead of Playwright's API request
            var httpClient = Fixture.Client;

            // Act
            var response = await httpClient.GetAsync("chat?message=");
            var statusCode = (int)response.StatusCode;
            var content = await response.Content.ReadAsStringAsync();

            // Log for debugging
            _output.WriteLine($"Status: {statusCode} {response.StatusCode}");
            _output.WriteLine($"Content: {content}");

            // Assert
            statusCode.Should().Be(400);
            content.Should().Contain("Message is required");
        }

        [Fact]
        public async Task DirectHttpClient_ShouldReturnBadRequest_WhenCalledWithEmptyMessage()
        {
            // This test bypasses Playwright to see if the issue is in our test setup or the API
            // Arrange
            var httpClient = Fixture.Client;

            // Act
            var response = await httpClient.GetAsync("chat?message=");
            var statusCode = (int)response.StatusCode;
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Status: {statusCode} {response.StatusCode}");
            _output.WriteLine($"Content: {content}");

            // Assert
            statusCode.Should().Be(400, "API should return Bad Request for empty message");
        }
    }

    // Response class for deserializing the chat response
    public class ChatResponse
    {
        public string Text { get; set; } = string.Empty;
    }
}
