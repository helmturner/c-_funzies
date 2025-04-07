using System.Diagnostics;
using FluentAssertions;
using Microsoft.Playwright;

namespace funzies.Tests.E2E;

public class PerformanceTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
{
    [Fact]
    public async Task HomePage_ShouldLoad_Successfully()
    {
        fixture.ApplicationFactory.Server.BaseAddress = new Uri(BaseUrl);
        // Arrange & Act
        var response = await Page.GotoAsync(BaseUrl);

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
        await Page.GotoAsync(BaseUrl);

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
    }

    [Fact]
    public async Task HomePage_LoadTime_ShouldBeUnderThreshold()
    {
        // Arrange
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        var response = await Page.GotoAsync(
            BaseUrl,
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle }
        );
        stopwatch.Stop();

        // Assert
        response.Should().NotBeNull();
        response!.Ok.Should().BeTrue();
        stopwatch
            .ElapsedMilliseconds.Should()
            .BeLessThan(3000, "Page should load in less than 3 seconds");
    }

    [Fact]
    public async Task ChatResponse_ShouldCompleteWithinTimeout()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act - Start timer, send message, and wait for response
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        await Page.FillAsync("#chatInput", "Give me a short response");
        await Page.ClickAsync("#sendButton");

        // Wait for the AI response
        await Page.WaitForSelectorAsync(".ai.message", new() { Timeout = 10000 });
        stopwatch.Stop();

        // Assert
        stopwatch
            .ElapsedMilliseconds.Should()
            .BeLessThan(10000, "AI should respond within 10 seconds");
    }

    [Fact]
    public async Task Application_ShouldBeAccessible()
    {
        // Arrange

        await Page.GotoAsync(BaseUrl);

        // Act
        var inputField = await Page.QuerySelectorAsync("#chatInput");
        var ariaLabel = await inputField!.GetAttributeAsync("aria-label");
        var placeholder = await inputField.GetAttributeAsync("placeholder");

        // Assert
        inputField.Should().NotBeNull("Message input field should exist");
        // Either an aria-label or placeholder should be present for accessibility
        (ariaLabel != null || placeholder != null)
            .Should()
            .BeTrue("Input field should have aria-label or placeholder for accessibility");
    }
}
