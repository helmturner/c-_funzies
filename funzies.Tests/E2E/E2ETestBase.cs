using Microsoft.Playwright;

namespace funzies.Tests.E2E;

public class E2ETestBase(PlaywrightFixture fixture)
    : IClassFixture<PlaywrightFixture>,
        IAsyncLifetime
{
    protected readonly PlaywrightFixture Fixture = fixture;
    protected IBrowserContext Context = null!;
    protected IPage Page = null!;
    protected string BaseUrl => Fixture.BaseUrl;

    public async Task InitializeAsync()
    {
        if (Fixture.Browser == null)
        {
            throw new Exception("Browser is not initialized.");
        }

        // Create a new browser context with more permissive settings
        Context = await Fixture.Browser.NewContextAsync(
            new BrowserNewContextOptions
            {
                BaseURL = BaseUrl,
                RecordVideoDir = "videos/",
                RecordVideoSize = new() { Width = 1280, Height = 720 },
                ViewportSize = new() { Width = 1280, Height = 720 },
                IgnoreHTTPSErrors = true, // Ignore HTTPS errors which might cause 403s
                ExtraHTTPHeaders = new Dictionary<string, string>
                {
                    // Add headers that might help with authorization issues
                    { "Accept", "*/*" },
                    {
                        "User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
                    },
                },
            }
        );

        // Create a new page in the browser context
        Page = await Context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (Context != null)
        {
            await Context.DisposeAsync();
        }
    }
}
