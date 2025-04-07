using Microsoft.Playwright;

namespace funzies.Tests.E2E;

public class E2ETestBase(PlaywrightFixture fixture)
    : IClassFixture<PlaywrightFixture>,
        IAsyncLifetime
{
    protected IBrowserContext Context = null!;
    protected IPage Page = null!;
    protected string BaseUrl => fixture.Client.BaseAddress!.ToString();

    public async Task InitializeAsync()
    {
        if (fixture.Browser == null)
        {
            throw new Exception("Browser is not initialized.");
        }

        // Create a new browser context
        Context = await fixture.Browser.NewContextAsync(
            new BrowserNewContextOptions
            {
                BaseURL = BaseUrl,
                RecordVideoDir = "videos/",
                RecordVideoSize = new() { Width = 1280, Height = 720 },
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
