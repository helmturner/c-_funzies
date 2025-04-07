using funzies.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace funzies.Tests.Integration
{
    /// <summary>
    /// Custom WebApplicationFactory for integration tests.
    /// This allows us to create a test server with custom services/configurations
    /// </summary>
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(configBuilder =>
            {
                // Add test configuration
                configBuilder.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        { "OpenAI:ApiKey", "test-api-key-for-integration-tests" },
                    }
                );
            });

            builder.ConfigureServices(services =>
            {
                // Here we can replace real services with test doubles if needed
                // For example, replace the real OpenAIService with a mock version

                // Remove the real OpenAIService
                var openAIServiceDescriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(IOpenAIService)
                );

                if (openAIServiceDescriptor != null)
                {
                    services.Remove(openAIServiceDescriptor);
                }

                // Add a mock OpenAIService
                var mockOpenAIService = new Mock<IOpenAIService>();
                mockOpenAIService
                    .Setup(m => m.GenerateChatCompletionAsync(It.IsAny<string>()))
                    .ReturnsAsync("This is a test response from the mock OpenAI service.");

                services.AddScoped(_ => mockOpenAIService.Object);
            });
        }
    }
}
