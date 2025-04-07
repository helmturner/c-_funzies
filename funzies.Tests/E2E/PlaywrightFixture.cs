using System.Configuration;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;

namespace funzies.Tests.E2E;

/// <summary>
/// This fixture sets up the application and a browser for E2E testing
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program>? ApplicationFactory;
    protected TestServer? _server;
    public readonly int _port = int.Parse("5231");
    public string BaseUrl => $"http://localhost:{_port}";

    public IPlaywright? Playwright;
    public IBrowser? Browser;

    public required HttpClient Client;

    public async Task InitializeAsync()
    {
        // Create the application factory and configure it for testing
        ApplicationFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");

            // Configure services for testing environment
            builder.ConfigureServices(services =>
            {
                // Add any test-specific service configurations here if needed
            });

            // Configure Kestrel to use a specific port for testing
            builder.UseKestrel(options =>
            {
                options.Listen(IPAddress.Loopback, _port);
            });
        });

        _server = ApplicationFactory.Server;
        _server.BaseAddress = new Uri(BaseUrl);

        // Create a client with specific handler options to follow redirects
        Client = ApplicationFactory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true,
                HandleCookies = true,
                MaxAutomaticRedirections = 7,
            }
        );

        Client.BaseAddress = new Uri(BaseUrl);

        // Warm up the server
        var response = await Client.GetAsync("/");
        Console.WriteLine($"Server warmup response: {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Server failed to start: {response.StatusCode}");
        }
        Console.WriteLine("Server started successfully");

        // Install and initialize Playwright
        try
        {
            var exitCode = Microsoft.Playwright.Program.Main(["install", "--with-deps"]);
            if (exitCode != 0)
            {
                throw new Exception($"Playwright install failed with exit code {exitCode}");
            }

            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    SlowMo = 50,
                    Args = new[] { "--disable-web-security", "--allow-insecure-localhost" }, // Disable security for testing
                }
            );

            Console.WriteLine($"Playwright browser initialized and pointed to {BaseUrl}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing Playwright: {ex.Message}");
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        // Clean up resources in reverse order
        if (Browser != null)
        {
            await Browser.DisposeAsync();
        }

        Playwright?.Dispose();

        if (ApplicationFactory != null)
        {
            await ApplicationFactory.DisposeAsync();
        }
    }
}
