using FluentAssertions;
using Microsoft.Playwright;

namespace funzies.Tests.E2E;

public class WebUITests : E2ETestBase
{
    public WebUITests(PlaywrightFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task HomePage_ShouldLoad_Successfully()
    {
        // Arrange & Act
        var response = await Page.GotoAsync(BaseUrl);

        // Assert
        response.Should().NotBeNull();
        response!.Ok.Should().BeTrue();

        // Verify page title
        var title = await Page.TitleAsync();
        title.Should().Contain("Chat Application");

        // Verify UI elements
        var chatContainer = await Page.QuerySelectorAsync("#chatContainer");
        chatContainer.Should().NotBeNull();

        var messageInput = await Page.QuerySelectorAsync("#messageInput");
        messageInput.Should().NotBeNull();

        var sendButton = await Page.QuerySelectorAsync("#sendButton");
        sendButton.Should().NotBeNull();
    }

    [Fact]
    public async Task ChatUI_ShouldSendMessage_AndReceiveResponse()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Initial message count should be 0
        var initialMessageCount = await Page.QuerySelectorAllAsync(".message");
        initialMessageCount.Count.Should().Be(0);

        // Act - Type a message and click send
        await Page.FillAsync("#messageInput", "Hello, AI assistant!");
        await Page.ClickAsync("#sendButton");

        // Wait for the user message to appear
        await Page.WaitForSelectorAsync(".message.user");

        // Wait for the AI response (this may take some time)
        await Page.WaitForSelectorAsync(".message.ai", new() { Timeout = 10000 });

        // Assert
        var messages = await Page.QuerySelectorAllAsync(".message");
        messages.Count.Should().BeGreaterThanOrEqualTo(2); // At least one user message and one AI response

        var userMessage = await Page.QuerySelectorAsync(".message.user");
        userMessage.Should().NotBeNull();

        var userMessageText = await userMessage!.TextContentAsync();
        userMessageText.Should().Contain("Hello, AI assistant!");

        var aiMessage = await Page.QuerySelectorAsync(".message.ai");
        aiMessage.Should().NotBeNull();

        var aiMessageText = await aiMessage!.TextContentAsync();
        aiMessageText.Should().NotBeNullOrEmpty();
    }
}
