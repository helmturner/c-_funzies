using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace funzies.Tests.E2E
{
    /// <summary>
    /// A test fixture that manages the lifecycle of Playwright and the ASP.NET Core test server.
    /// </summary>
    public class PlaywrightFixture : IDisposable
    {
        public WebApplicationFactory<Program> Factory { get; }
        public IPlaywright? Playwright { get; private set; }
        public IBrowser? Browser { get; private set; }
        public string ServerUrl { get; }

        private bool _disposed;

        public PlaywrightFixture()
        {
            // Start the web server
            Factory = new WebApplicationFactory<Program>();
            var client = Factory.CreateClient();
            ServerUrl = client.BaseAddress?.ToString() ?? "http://localhost:5231/";
        }

        public async Task InitializeAsync()
        {
            // Initialize Playwright
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions { Headless = true }
            );
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

    [TestClass]
    public abstract class PlaywrightTestBase
    {
        protected PlaywrightFixture? Fixture { get; private set; }

        [TestInitialize]
        public virtual async Task TestInitialize()
        {
            Fixture = new PlaywrightFixture();
            await Fixture.InitializeAsync();
        }

        [TestCleanup]
        public virtual void TestCleanup()
        {
            Fixture?.Dispose();
            Fixture = null;
        }
    }
}
