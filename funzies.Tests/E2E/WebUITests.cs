using FluentAssertions;

namespace funzies.Tests.E2E;

public class WebUITests(PlaywrightFixture fixture) : E2ETestBase(fixture)
{
    [Fact]
    public async Task HomePage_ShouldLoad_Successfully()
    {
        // Arrange & Act
        var response = await Page.GotoAsync(fixture.Client.BaseAddress!.ToString());

        // Assert
        response.Should().NotBeNull();
        response!.Ok.Should().BeTrue();

        // Verify page title
        var title = await Page.TitleAsync();
        title.Should().Contain("Canyon Trail Chat");

        // Verify UI elements
        var chatContainer = await Page.QuerySelectorAsync("#chatContainer");
        chatContainer.Should().NotBeNull();

        var chatInput = await Page.QuerySelectorAsync("#chatInput");
        chatInput.Should().NotBeNull();

        var sendButton = await Page.QuerySelectorAsync("#sendButton");
        sendButton.Should().NotBeNull();
    }

    [Fact]
    public async Task ChatUI_ShouldSendMessage_AndReceiveResponse()
    {
        // Arrange
        await Page.GotoAsync(fixture.Client.BaseAddress!.ToString());

        // Initial message count should be 1, the initial prompt.
        var initialMessageCount = await Page.QuerySelectorAllAsync(".message");
        initialMessageCount.Count.Should().Be(1);

        // Act - Type a message and click send
        await Page.FillAsync("#chatInput", "Hello, AI assistant!");
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
