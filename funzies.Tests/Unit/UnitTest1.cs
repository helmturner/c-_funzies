using FluentAssertions;
using funzies.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace funzies.Tests.Unit
{
    public class OpenAIServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<OpenAIService>> _mockLogger;

        public OpenAIServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<OpenAIService>>();

            // Setup the Configuration mock to return a valid API key
            _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-api-key");
        }

        [Fact]
        public void Constructor_WithValidApiKey_CreatesInstance()
        {
            // Arrange & Act
            var service = new OpenAIService(_mockConfiguration.Object, _mockLogger.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithInvalidApiKey_ThrowsArgumentNullException()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns((string)null!);

            // Act & Assert
            Action act = () => new OpenAIService(_mockConfiguration.Object, _mockLogger.Object);
            act.Should()
                .Throw<ArgumentNullException>()
                .WithMessage("*OpenAI API key is not configured*");
        }

        // Note: Testing the actual GenerateChatCompletionAsync would require mocking the OpenAI client,
        // which is more complex. In a real-world scenario, you might want to introduce a wrapper or adapter
        // around the OpenAI client to facilitate easier testing.
    }
}
