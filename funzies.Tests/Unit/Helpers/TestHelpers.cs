using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace funzies.Tests.Unit.Helpers
{
    public static class TestHelpers
    {
        /// <summary>
        /// Creates a service collection with mocked dependencies for unit testing.
        /// </summary>
        /// <param name="configureServices">Optional action to configure additional services.</param>
        /// <returns>A configured ServiceProvider for unit testing.</returns>
        public static IServiceProvider CreateTestServiceProvider(
            Action<IServiceCollection>? configureServices = null
        )
        {
            var services = new ServiceCollection();

            // Add mock configuration
            var mockConfiguration = new Mock<IConfiguration>();
            services.AddSingleton(mockConfiguration.Object);

            // Configure additional services if needed
            configureServices?.Invoke(services);

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Creates a configuration builder with test settings
        /// </summary>
        /// <returns>An IConfiguration with test settings</returns>
        public static IConfiguration CreateTestConfiguration()
        {
            var initialData = new List<KeyValuePair<string, string?>>
            {
                new KeyValuePair<string, string?>("OpenAI:ApiKey", "test-api-key"),
            };

            return new ConfigurationBuilder().AddInMemoryCollection(initialData).Build();
        }
    }
}
