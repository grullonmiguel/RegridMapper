using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using RegridMapper.Core.Configuration;
using RegridMapper.Core.Utilities;
using SeleniumExtras.WaitHelpers;
using System.Diagnostics;
using System.Net.Http;

namespace RegridMapper.Services
{
    public class SeleniumWebDriverService : IDisposable
    {
        private bool _disposed = false;
        private IWebDriver? _driver;
        private readonly Logger _logger;
        private WebDriverWait _wait;

        public IWebDriver? WebDriver => _driver; // Read-only accessor

        /// <summary>
        /// Initializes the Selenium WebDriver with the specified browser.
        /// Default is Chrome.
        /// </summary>
        public SeleniumWebDriverService(BrowserType browser = BrowserType.Chrome, bool headless = true, string? debuggerAddress = null)
        {
            _logger = Logger.Instance;

            // Validate the debugger address
            if (!string.IsNullOrWhiteSpace(debuggerAddress) && !debuggerAddress.IsValidFormat(AppConstants.RegexIPPort))
                debuggerAddress = string.Empty;

            _driver = CreateWebDriver(browser, headless, debuggerAddress);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
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
                    chromeOptions.AddArgument("--window-size=1280,720");
                    chromeOptions.AddArgument("--no-sandbox");
                    chromeOptions.AddArgument("--ignore-certificate-errors");
                    chromeOptions.AddArgument("--disable-popup-blocking");
                    chromeOptions.AddArgument("--disable-infobars");
                    chromeOptions.AddArgument("--disable-background-timer-throttling");
                    chromeOptions.AddArgument("--disable-backgrounding-occluded-windows");
                    chromeOptions.AddArgument("--disable-renderer-backgrounding");
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
        public async Task<string?> CaptureHTMLSource(string parcelUrl)
        {
            // Ensure WebDriver process is active
            if (!IsWebDriverRunning() || _driver == null) 
            {
                await Task.Run(() => _logger.LogAsync("WebDriver process is not running. Skipping request."));
                return null;
            }

            try
            {
                // Simple call to check if WebDriver is still responsiveby accessing the title
                _ = _driver.Title;

                // It behaves like manually typing the URL into a browser and pressing Enter.
                _driver.Navigate().GoToUrl(parcelUrl);

                // Captures and returns the HTML content of the page.
                return _driver.PageSource;
            }
            catch (WebDriverException ex)
            {
                await Task.Run(() => _logger.LogExceptionAsync(ex));
                return null;
            }
        }

        public async Task<List<string>> CaptureHTMLTable()
        {
            var data = new List<string>();

            try
            {
                // Ensure page is fully loaded before scraping
                await Task.Delay(300);

                // Find elements asynchronously
                var elements = await Task.Run(() => _driver.FindElements(By.XPath("//table//tr")));

                // Extract data asynchronously using parallel processing
                var extractedItems = await Task.Run(() => elements.AsParallel().Select(element => element.Text.Trim())
                    .Where(text => !string.IsNullOrEmpty(text) &&
                                   text.StartsWith("USER NAME", StringComparison.OrdinalIgnoreCase))
                    .ToList());

                data.AddRange(extractedItems);
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("No matching elements found.");
            }
            catch (WebDriverException)
            {
                Console.WriteLine("WebDriver error occurred.");
            }
            catch (Exception)
            {
                Console.WriteLine("Unexpected error occurred.");
            }

            return data;
        }

        /// <summary>
        /// Retry Mechanism for Clicking Next Page – Handles transient failures.
        /// </summary>
        public bool TryClickNextPage(int maxRetries = 3)
        {
            int attempts = 0;
            while (attempts < maxRetries)
            {
                try
                {
                    var nextPageButton = _wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".PageRight")));
                    nextPageButton?.Click();
                    return true;
                }
                catch (TimeoutException)
                {
                    Console.WriteLine($"Attempt {attempts + 1}: Next page button not clickable.");
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine($"Attempt {attempts + 1}: Next page button not found.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {attempts + 1}: Next page button not found.");
                }

                attempts++;
            }

            return false;
        }

        public async Task NavigateToUrl(string url)
        {
            try
            {
                _driver.Navigate().GoToUrl(url);
            }
            catch (WebDriverException ex)
            {
                await Task.Run(() => _logger.LogExceptionAsync(ex));
            }
        }

        // Verify WebDriver process is running
        public bool IsWebDriverRunning()
        {
            var processes = Process.GetProcessesByName("chromedriver"); // Change this to match your WebDriver executable
            if (processes.Length == 0)
            {
                _logger.LogAsync("WebDriver process is not running.");
                return false;
            }

            return true;
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
                throw new ObjectDisposedException(nameof(SeleniumWebDriverService), "WebDriver instance has been disposed.");

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