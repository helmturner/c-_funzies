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
            // Arrange
            var context = await Fixture.Browser!.NewContextAsync();
            try
            {
                var page = await context.NewPageAsync();

                // Log what we're testing for debug purposes
                _output.WriteLine($"Testing endpoint: {Fixture.ServerUrl}chat?message=Hello");

                // Construct the URL to test
                var url = $"{Fixture.ServerUrl}chat?message=Hello";

                // Act - Send API request
                var response = await page.APIRequest.GetAsync(url);
                _output.WriteLine($"Response status: {response.Status}");

                // Assert
                response
                    .Ok.Should()
                    .BeTrue(
                        $"Expected OK response but got {response.Status} {response.StatusText}"
                    );

                var responseJson = await response.JsonAsync();
                _output.WriteLine($"Response JSON: {responseJson}");

                var responseText = responseJson.Value.GetProperty("text").GetString();
                responseText.Should().NotBeNull();
                responseText!.Should().NotBeEmpty();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Test failed with exception: {ex}");
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task ChatEndpoint_ShouldReturnBadRequest_WhenCalledWithEmptyMessage()
        {
            // Arrange
            var context = await Fixture.Browser!.NewContextAsync();
            try
            {
                var page = await context.NewPageAsync();

                // Log what we're testing for debug purposes
                _output.WriteLine($"Testing endpoint: {Fixture.ServerUrl}chat?message=");

                // Construct the URL to test (empty message)
                var url = $"{Fixture.ServerUrl}chat?message=";

                // Act - Send API request
                var response = await page.APIRequest.GetAsync(url);
                _output.WriteLine($"Response status: {response.Status}");

                // Assert
                response.Ok.Should().BeFalse("Expected error response for empty message");
                response.Status.Should().Be(400, "Expected 400 Bad Request for empty message");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Test failed with exception: {ex}");
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }
    }
}
