using System.Diagnostics;
using FluentAssertions;
using Microsoft.Playwright;

namespace funzies.Tests.E2E;

public class PerformanceTests : E2ETestBase
{
    public PerformanceTests(PlaywrightFixture fixture)
        : base(fixture) { }

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
        
        await Page.FillAsync("#messageInput", "Give me a short response");
        await Page.ClickAsync("#sendButton");
        
        // Wait for the AI response
        await Page.WaitForSelectorAsync(".message.ai", new() { Timeout = 10000 });
        stopwatch.Stop();
        
        // Assert
        stopwatch
            .ElapsedMilliseconds.Should()
            .BeLessThan(10000, "AI should respond within 10 seconds");
    }

    [Fact]
    public async Task Application_ShouldBeAccessible()
    {
        // Arrange & Act
        await Page.GotoAsync(BaseUrl);
        
        // Get accessibility snapshot
        var snapshot = await Page.AccessibilitySnapshotAsync();
        
        // Assert
        // Check for basic accessibility features
        var inputField = await Page.QuerySelectorAsync("#messageInput");
        var ariaLabel = await inputField.GetAttributeAsync("aria-label");
        
        // Verify accessibility attributes are present
        ariaLabel.Should().NotBeNullOrEmpty("Input field should have aria-label for accessibility");
    }
}