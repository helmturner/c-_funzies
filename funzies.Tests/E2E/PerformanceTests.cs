using System.Diagnostics;
using FluentAssertions;
using Microsoft.Playwright;

namespace funzies.Tests.E2E;

public class PerformanceTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
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

        // Verify the initial message is a system message
        var systemMessage = await Page.QuerySelectorAsync(".message.system");
        systemMessage.Should().NotBeNull("There should be a system welcome message");

        // Act - Type a message and click send
        await Page.FillAsync("#chatInput", "Hello, AI assistant!");
        await Page.ClickAsync("#sendButton");

        // Wait for a second message to appear (this would be the user message)
        await Page.WaitForSelectorAsync(".message:nth-child(2)", new() { Timeout = 10000 });

        // Assert - Check that we now have at least 2 messages
        var messages = await Page.QuerySelectorAllAsync(".message");
        messages.Count.Should().BeGreaterThanOrEqualTo(2);
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
        await Page.WaitForSelectorAsync(
            "#chatInput",
            new() { State = WaitForSelectorState.Visible }
        );

        // Act - Start timer, send message, and wait for response
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        await Page.FillAsync("#chatInput", "Give me a short response");
        await Page.ClickAsync("#sendButton");

        // Wait for a second message to appear after the system welcome
        await Page.WaitForSelectorAsync(".message:nth-child(2)", new() { Timeout = 10000 });

        // Then wait for a third message which should be the AI response
        await Page.WaitForSelectorAsync(".message:nth-child(3)", new() { Timeout = 10000 });
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
        await Page.WaitForSelectorAsync(
            "#chatInput",
            new() { State = WaitForSelectorState.Visible }
        );

        // Act
        var inputField = await Page.QuerySelectorAsync("#chatInput");

        // Assert
        inputField.Should().NotBeNull("Message input field should exist");

        // Check for accessibility attributes
        var ariaLabel = await inputField!.GetAttributeAsync("aria-label");
        var placeholder = await inputField.GetAttributeAsync("placeholder");

        // Either an aria-label or placeholder should be present for accessibility
        (ariaLabel != null || placeholder != null)
            .Should()
            .BeTrue("Input field should have aria-label or placeholder for accessibility");
    }
}
