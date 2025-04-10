using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using RegridMapper.Core.Configuration;
using RegridMapper.Core.Utilities;

namespace RegridMapper.Services
{
    public class SeleniumScraper : IDisposable
    {
        private bool _disposed = false;
        private IWebDriver? _driver;

        public IWebDriver? WebDriver => _driver; // Read-only accessor

        /// <summary>
        /// Initializes the Selenium WebDriver with the specified browser.
        /// Default is Chrome.
        /// </summary>
        public SeleniumScraper(BrowserType browser = BrowserType.Chrome, bool headless = true, string? debuggerAddress = null)
        {
            // Validate the debugger address
            if (!string.IsNullOrWhiteSpace(debuggerAddress) && !debuggerAddress.IsValidFormat(AppConstants.RegexIPPort))
                debuggerAddress = string.Empty;

            _driver = CreateWebDriver(browser, headless, debuggerAddress);
        }

        private IWebDriver CreateWebDriver(BrowserType browser, bool headless, string? debuggerAddress)
        {
            switch (browser)
            {
                case BrowserType.Firefox:
                    var firefoxOptions = new FirefoxOptions();
                    if (headless) firefoxOptions.AddArgument("--headless");
                    //if (!string.IsNullOrWhiteSpace(debuggerAddress)) firefoxOptions.DebuggerAddress = debuggerAddress;
                    return new FirefoxDriver(firefoxOptions);

                case BrowserType.Edge:
                    var edgeOptions = new EdgeOptions();
                    if (headless) edgeOptions.AddArgument("headless");
                    if (!string.IsNullOrWhiteSpace(debuggerAddress)) edgeOptions.DebuggerAddress = debuggerAddress;
                    return new EdgeDriver(edgeOptions);

                default: // Chrome
                    var chromeOptions = new ChromeOptions();
                    if (headless) chromeOptions.AddArgument("--headless");
                    chromeOptions.AddArgument("--disable-gpu");
                    chromeOptions.AddArgument("--disable-extensions");
                    chromeOptions.AddArgument("--blink-settings=imagesEnabled=false");
                    chromeOptions.AddArgument("--window-size=1920,1080");
                    chromeOptions.AddArgument("--no-sandbox");
                    chromeOptions.AddArgument("--ignore-certificate-errors");
                    chromeOptions.AddArgument("--disable-popup-blocking");
                    chromeOptions.AddArgument("--disable-infobars");
                    chromeOptions.AddArgument("--disable-background-timer-throttling");
                    chromeOptions.AddArgument("--disable-backgrounding-occluded-windows");
                    chromeOptions.AddArgument("--disable-sync");
                    chromeOptions.PageLoadStrategy = PageLoadStrategy.Eager;

                    if (!string.IsNullOrWhiteSpace(debuggerAddress))
                        chromeOptions.DebuggerAddress = debuggerAddress;

                    return new ChromeDriver(chromeOptions);
            }
        }

        /// <summary>
        /// Navigates to the specified URL and returns the page source.
        /// </summary>
        public string ScrapeParcelData(string parcelUrl)
        {
            if (_driver == null)
                throw new ObjectDisposedException(nameof(SeleniumScraper), "WebDriver instance has been disposed.");

            _driver.Navigate().GoToUrl(parcelUrl);
            return _driver.PageSource;
        }

        /// <summary>
        /// Safely disposes the WebDriver instance.
        /// </summary>
        public void SafeDispose()
        {
            if (_disposed) return;

            try
            {
                _driver?.Quit();
                _driver?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while disposing WebDriver: {ex.Message}");
            }
            finally
            {
                _driver = null;
                _disposed = true;
            }
        }

        public IWebElement? FindElementSafely(By locator)
        {
            if (_driver == null)
                throw new ObjectDisposedException(nameof(SeleniumScraper), "WebDriver instance has been disposed.");

            try
            {
                return _driver.FindElement(locator);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        public void Dispose()
        {
            SafeDispose();
            GC.SuppressFinalize(this);
        }
    }
}