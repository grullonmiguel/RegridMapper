using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using RegridMapper.Core.Configuration;
using RegridMapper.Services;
using RegridMapper.ViewModels;
using System.Text.RegularExpressions;

namespace RegridMapper.Core.Services
{
    public class RegridDataService
    {
        #region CanScrape Properties
        public bool ShouldScrapeCity { get; set; } = true;
        public bool ShouldScrapeZoning { get; set; } = true;
        public bool ShouldScrapeZipCode { get; set; } = true;
        public bool ShouldScrapeAddress { get; set; } = true;
        public bool ShouldScrapeOwner { get; set; } = true;
        public bool ShouldScrapeAssessedValue { get; set; } = true;
        public bool ShouldScrapeAcres { get; set; } = true;
        public bool ShouldScrapeCoordinates { get; set; } = true;
        public bool ShouldScrapeFloodZone { get; set; } = true;
        #endregion

        public RegridDataService()
        {
                
        }

        public async Task GetParcelData(string htmlSource, ParcelData item, SeleniumWebDriverService scraper)
        {
            try
            {
                // Find number of matches returned
                var matchCount = GetRegridMatchCount(htmlSource);

                // Handle match count logic separately
                if (!ResultsFound(matchCount, item)) return;

                //await Task.Delay(300); // Allow page transition

                var wait = new WebDriverWait(scraper.WebDriver, TimeSpan.FromSeconds(5));
                var divContainer = wait.Until(d => scraper.FindElementSafely(By.CssSelector("div.headline.parcel-result")));
                if (divContainer == null)
                {
                    item.ZoningType = $"Parcel result not found - {item.ParcelID}";
                    await Task.Delay(300);
                    return;
                }

                // Capture the current page URL before clicking
                string previousUrl = scraper.WebDriver.Url;

                // Click the Hyperlink and Navigate to parcel details page
                var js = (IJavaScriptExecutor)scraper.WebDriver;
                var element = divContainer.FindElement(By.TagName("a"));
                js.ExecuteScript("arguments[0].click();", element);

                //divContainer.FindElement(By.TagName("a")).Click();
                //wait = new WebDriverWait(scraper.WebDriver, TimeSpan.FromSeconds(2));

                // Wait for the URL to change (indicating a page transition)
                wait.Until(d => d.Url != previousUrl);

                // Allow page transition
                //wait = new WebDriverWait(scraper.WebDriver, TimeSpan.FromSeconds(2));

                // Update the Regrid URL
                item.RegridUrl = scraper.WebDriver.Url;

                // Scrape the individual items
                await UpdateElement(item, "ZoningType", ShouldScrapeZoning, AppConstants.RegridZoningType, scraper, AppConstants.RegridZoningType, "Zoning type", "Zoning Description", "Land Use");
                await UpdateElement(item, "City", ShouldScrapeCity, AppConstants.RegridCity, scraper, AppConstants.RegridCity);
                await UpdateElement(item, "ZipCode", ShouldScrapeZipCode, AppConstants.RegridZip, scraper, AppConstants.RegridZip, AppConstants.RegridZip2);
                await UpdateElement(item, "Acres", ShouldScrapeAcres, AppConstants.RegridAcres, scraper, AppConstants.RegridAcres);
                await UpdateElement(item, "Address", ShouldScrapeAddress, AppConstants.RegridAddress, scraper, AppConstants.RegridAddress);
                await UpdateElement(item, "OwnerName", ShouldScrapeOwner, AppConstants.RegridOwner, scraper, AppConstants.RegridOwner, AppConstants.RegridOwner);
                await UpdateElement(item, "AssessedValue", ShouldScrapeAssessedValue, AppConstants.RegridAssessedValue, scraper, AppConstants.RegridAssessedValue);
                await UpdateElement(item, "GeographicCoordinate", ShouldScrapeCoordinates, AppConstants.RegridCoordinates, scraper, AppConstants.RegridCoordinates);
                await UpdateElement(item, "FloodZone", ShouldScrapeFloodZone, AppConstants.RegridFema, scraper, AppConstants.RegridFema, AppConstants.RegridFema2, "N/A");

                // Format the acres value by removing the word acres
                if (!string.IsNullOrEmpty(item.Acres))
                    item.Acres = item.Acres.ToLower().Replace(" acres", "");

                item.ScrapeStatus =  ScrapeStatus.Complete;
            }
            catch (WebDriverException ex)
            {
                //CurrentScrapingElement = $"Web scraping error: {ex.Message}";
            }
            catch (Exception ex)
            {
                //CurrentScrapingElement = $"Unexpected error: {ex.Message}";
            }
        }

        /// <summary>
        /// FInd how many matches returned for a parcel search
        /// </summary>
        private int GetRegridMatchCount(string pageSource)
            => int.TryParse(Regex.Match(pageSource, @"Found (\d+) matches").Groups[1].Value, out int count) ? count : -1;

        /// <summary>
        /// Helper: Handle match count logic separately
        /// </summary>
        private bool ResultsFound(int results, ParcelData item)
        {
            if (results == 0 || results > 1)
            {
                item.ZoningType = results == 0 ? $"Not found" : $"Multiple matches";
                item.ScrapeStatus = results == 0? ScrapeStatus.NotFound : ScrapeStatus.MultipleMatches;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Helper: Scrape property dynamically
        /// </summary>
        private async Task UpdateElement(ParcelData item, string propertyName, bool shouldScrape, string label, SeleniumWebDriverService scraper, params string[] fallbackLabels)
        {
            if (shouldScrape)
            {
                //UpdateRegridStatusLabel(item, label);
                var result = await FindElement(scraper.WebDriver, fallbackLabels);
                item.SetPropertyValue(propertyName, result);
            }
        }

        private async Task<string> FindElement(IWebDriver driver, params string[] selectors)
        {
            foreach (var selector in selectors)
            {
                var text = await FindElementContainer(driver, selector);
                if (!string.IsNullOrEmpty(text))
                    return text;
            }

            return string.Empty;
        }

        /// <summary>
        /// Attempts to retrieve the text content of a div element based on a 
        /// given search term, with built-in retries and fallback mechanisms.
        /// </summary>
        public async Task<string> FindElementContainer(IWebDriver driver, string searchText, int maxRetries = 5)
        {
            try
            {
                IWebElement target = null;
                IWebElement nextDiv = null;

                // Attempt to find the target element (limited retries)
                int retryCount = 0;
                while (target == null && retryCount < maxRetries)
                {
                    try
                    {
                        target = driver.FindElement(By.XPath($"//div[contains(text(), '{searchText}')]"));
                    }
                    catch (NoSuchElementException)
                    {
                        await Task.Delay(200); // Reduce unnecessary wait times
                        retryCount++;
                    }
                }

                if (target == null)
                    return ""; // Exit early if target not found

                // Locate the next sibling div efficiently
                nextDiv = TryFindElement(target, By.XPath("following-sibling::div")) ??
                          TryFindElement(target, By.XPath("ancestor::div/following-sibling::div[contains(@class, 'field-value')]//span"));

                return nextDiv?.Text.Replace("\r\n", " ") ?? "Not found";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetNextDivTextAsync: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// Helper method to safely locate elements
        /// </summary>
        private IWebElement TryFindElement(IWebElement parent, By by)
        {
            try
            {
                return parent.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                return null; // Avoid unnecessary exception propagation
            }
        }
    }
}
