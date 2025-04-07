using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace funzies.Tests.Integration
{
    public class ChatEndpointTests(TestWebApplicationFactory factory)
        : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory = factory;

        [Fact]
        public async Task GetChatCompletion_WithValidMessage_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var message = "Hello, how are you?";

            // Act
            var response = await client.PostAsJsonAsync("/chat", new { message });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadFromJsonAsync<ChatResponse>();
            content.Should().NotBeNull();
            content!.Text.Should().Be("This is a test response from the mock OpenAI service.");
        }

        [Fact]
        public async Task GetChatCompletion_WithEmptyMessage_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/chat", new { message = "" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    public class ChatResponse
    {
        public string? Text { get; set; }
    }
}
