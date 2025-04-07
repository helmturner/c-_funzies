using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace funzies.Tests.Unit
{
    /// <summary>
    /// Base class for unit tests providing common setup and helper methods
    /// </summary>
    public abstract class TestBase : IDisposable
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly Mock<IConfiguration> MockConfiguration;
        protected readonly Mock<ILogger> MockLogger;

        protected TestBase()
        {
            var services = new ServiceCollection();

            // Create mocks
            MockConfiguration = new Mock<IConfiguration>();
            MockLogger = new Mock<ILogger>();

            // Setup configuration with test values
            MockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-api-key");

            // Register common services
            services.AddSingleton(MockConfiguration.Object);
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            // Configure services in derived classes
            ConfigureServices(services);

            // Build the service provider
            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Template method for configuring services in derived test classes
        /// </summary>
        /// <param name="services">The service collection to configure</param>
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // Default implementation does nothing
        }

        /// <summary>
        /// Gets a service from the container
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve</typeparam>
        /// <returns>The requested service</returns>
        protected T GetService<T>()
            where T : notnull
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public virtual void Dispose()
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// Simple Logger implementation for testing
        /// </summary>
        private class Logger<T> : ILogger<T>
        {
            public IDisposable BeginScope<TState>(TState state)
            {
                return NullScope.Instance;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter
            )
            {
                // No-op for tests
            }

            private class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();

                public void Dispose() { }
            }
        }
    }
}
