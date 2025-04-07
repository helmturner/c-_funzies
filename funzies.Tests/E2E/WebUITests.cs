using FluentAssertions;

namespace funzies.Tests.E2E;

public class WebUITests(PlaywrightFixture fixture) : E2ETestBase(fixture)
{
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
        title.Should().Contain("Canyon Trail Chat");

        // Verify UI elements based on actual HTML structure
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
        await Page.GotoAsync(BaseUrl);

        // Initial message count should be 1 (the welcome system message)
        var initialMessageCount = await Page.QuerySelectorAllAsync(".message");
        initialMessageCount.Count.Should().Be(1);

        // Act - Type a message and click send
        await Page.FillAsync("#chatInput", "Hello, AI assistant!");
        await Page.ClickAsync("#sendButton");

        // Wait for the user message to appear
        await Page.WaitForSelectorAsync(".message.user", new() { Timeout = 10000 });

        // Wait for the AI response (this may take some time)
        await Page.WaitForSelectorAsync(".message.ai", new() { Timeout = 10000 });

        // Assert
        var systemMessage = await Page.QuerySelectorAllAsync(".message.system");
        systemMessage.Count.Should().Be(1, "There should be one system message");

        var systemMessageText = await systemMessage[0].TextContentAsync();
        systemMessageText.Should().NotBeNullOrEmpty("System message should not be empty");

        var userMessage = await Page.QuerySelectorAsync(".message.user");
        userMessage.Should().NotBeNull("There should be a user message");

        var userMessageText = await userMessage!.TextContentAsync();
        userMessageText.Should().Contain("Hello, AI assistant!");

        var aiMessage = await Page.QuerySelectorAsync(".message.ai");
        aiMessage.Should().NotBeNull("There should be an AI response message");

        var aiMessageText = await aiMessage!.TextContentAsync();
        aiMessageText.Should().NotBeNullOrEmpty("AI message should not be empty");
    }
}
