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
    public WebApplicationFactory<Program> ApplicationFactory;
    protected TestServer? _server;
    public readonly int _port = int.Parse(ConfigurationManager.AppSettings["TestPort"] ?? "5000");

    public IPlaywright? Playwright;
    public IBrowser? Browser;

    public required HttpClient Client;

    public async Task InitializeAsync()
    {
        ApplicationFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");

            // Configure Kestrel to use a dynamic port
            builder.UseKestrel(options =>
            {
                options.Listen(IPAddress.Loopback, _port);
            });
        });

        _server = ApplicationFactory.Server;
        _server.BaseAddress = new Uri($"http://localhost:{_port}");
        Client = _server.CreateClient();
        // Start the web server
        await Client.GetAsync("/");
        Console.WriteLine("Server started");

        if (Client.BaseAddress == null)
        {
            throw new InvalidOperationException("BaseAddress is null");
        }

        // Install Playwright browsers if needed and initialize
        Microsoft.Playwright.Program.Main(["install"]);

        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        Browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions { Headless = true }
        );
    }

    public async Task DisposeAsync()
    {
        if (Browser != null)
        {
            await Browser.DisposeAsync();
        }

        if (ApplicationFactory != null)
        {
            await ApplicationFactory.DisposeAsync();
        }
        Playwright?.Dispose();
    }
}
