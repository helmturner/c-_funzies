using Microsoft.Playwright;
using System.Threading.Tasks;

namespace funzies.Tests.E2E;

public class E2ETestBase : IClassFixture<PlaywrightFixture>, IAsyncLifetime
{
    protected readonly PlaywrightFixture Fixture;
    protected IBrowserContext Context;
    protected IPage Page;
    protected string BaseUrl => Fixture.ServerAddress;

    public E2ETestBase(PlaywrightFixture fixture)
    {
        Fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        // Create a new browser context for each test class
        Context = await Fixture.Browser.NewContextAsync();
        
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