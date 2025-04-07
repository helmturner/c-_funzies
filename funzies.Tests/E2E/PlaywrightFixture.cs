using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;

namespace funzies.Tests.E2E;

/// <summary>
/// This fixture sets up the application and a browser for E2E testing
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _factory;
    private string _baseUrl = "http://localhost:5321";
    private IPlaywright _playwright = null!;

    public IBrowser Browser { get; private set; } = null!;
    public string ServerAddress => _baseUrl;

    public async Task InitializeAsync()
    {
        // Create a factory with a real server configured for E2E testing
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            // Use the Test environment
            builder.UseEnvironment("Test");

            // Configure Kestrel to use a dynamic port
            builder.UseKestrel(options =>
            {
                var port = GetFreeTcpPort();
                options.Listen(IPAddress.Loopback, port);
                _baseUrl = $"http://localhost:{port}";
                Console.WriteLine($"Test server starting on {_baseUrl}");
            });

            // Add any additional configuration needed for testing
            builder.ConfigureServices(services =>
            {
                // You could replace services with test implementations here if needed
            });
        });

        // Start the server by making a request to it
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        Console.WriteLine($"Server ready with status code: {response.StatusCode}");

        // Set up Playwright
        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions { Headless = true }
        );
    }

    private static int GetFreeTcpPort()
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public async Task DisposeAsync()
    {
        Console.WriteLine("Disposing PlaywrightFixture...");

        if (Browser != null)
        {
            await Browser.DisposeAsync();
        }

        if (_factory != null)
        {
            await _factory.DisposeAsync();
        }

        _playwright?.Dispose();

        Console.WriteLine("PlaywrightFixture disposed.");
    }
}
