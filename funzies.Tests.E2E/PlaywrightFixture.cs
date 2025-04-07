using funzies.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Moq;

namespace funzies.Tests.E2E
{
    /// <summary>
    /// A test fixture that manages the lifecycle of Playwright and the ASP.NET Core test server.
    /// </summary>
    public class PlaywrightFixture : IAsyncLifetime, IDisposable
    {
        public WebApplicationFactory<Program> Factory { get; }
        public IPlaywright? Playwright { get; private set; }
        public IBrowser? Browser { get; private set; }
        public string ServerUrl { get; } = "http://localhost:5231";
        public HttpClient Client =>
            Factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                    HandleCookies = true,
                    BaseAddress = new Uri(ServerUrl),
                }
            );

        private readonly ILogger<PlaywrightFixture> _logger;
        private bool _disposed;

        public PlaywrightFixture()
        {
            // Create a logging provider for debugging
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("funzies", LogLevel.Debug)
                    .AddConsole();
            });

            _logger = loggerFactory.CreateLogger<PlaywrightFixture>();
            _logger.LogInformation("Initializing PlaywrightFixture");

            // Create a mock OpenAI service instead of relying on configuration
            var mockOpenAIService = new Mock<IOpenAIService>();
            mockOpenAIService
                .Setup(m => m.GenerateChatCompletionAsync(It.IsAny<string>()))
                .ReturnsAsync("This is a test response from the mocked OpenAI service");

            // Start the web server with explicit port configuration and debug logging
            Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseUrls(ServerUrl);
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                });

                // Replace the real OpenAI service with our mock
                builder.ConfigureServices(services =>
                {
                    // Remove the real service registration
                    var descriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(IOpenAIService)
                    );

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add our mock service
                    services.AddSingleton(mockOpenAIService.Object);

                    // Also configure API key just to be safe
                    services.PostConfigure<IConfiguration>(config =>
                    {
                        var configDictionary = new Dictionary<string, string>
                        {
                            { "OpenAI:ApiKey", "test-api-key-for-e2e-tests" },
                        };

                        var memoryConfig = new ConfigurationBuilder()
                            .AddInMemoryCollection(
                                configDictionary.Cast<KeyValuePair<string, string?>>()
                            )
                            .Build();

                        // Add our config to the existing config
                        var configInstance = services
                            .BuildServiceProvider()
                            .GetRequiredService<IConfiguration>();

                        ((IConfigurationBuilder)configInstance).AddConfiguration(memoryConfig);
                    });
                });
            });

            _logger.LogInformation(
                "PlaywrightFixture initialized with base URL: {ServerUrl}",
                ServerUrl
            );
        }

        public async Task InitializeAsync()
        {
            // Initialize Playwright
            _logger.LogInformation("Initializing Playwright");
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions { Headless = true }
            );
            _logger.LogInformation("Playwright initialized successfully");

            // Verify that the server is responsive
            try
            {
                var client = Factory.CreateClient();
                var response = await client.GetAsync("/");
                _logger.LogInformation(
                    "Server health check status: {StatusCode}",
                    response.StatusCode
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during server health check");
            }
        }

        public Task DisposeAsync()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Dispose managed resources
                if (Browser != null)
                {
                    Browser.CloseAsync().GetAwaiter().GetResult();
                    Browser.DisposeAsync().GetAwaiter().GetResult();
                }
                Playwright?.Dispose();
                Factory?.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public abstract class PlaywrightTestBase(PlaywrightFixture fixture)
        : IClassFixture<PlaywrightFixture>
    {
        protected readonly PlaywrightFixture Fixture = fixture;
    }
}
