using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using RegridMapper.Core.Configuration;
using RegridMapper.Models;
using RegridMapper.Services;
using RegridMapper.ViewModels;
using SeleniumExtras.WaitHelpers;
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
        { }

        public async Task GetParcelData(ScrapeType scrapeBy, string htmlSource, ParcelData item, SeleniumWebDriverService scraper)
        {
            try
            {
                // Find number of matches returned
                var matchCount = GetRegridMatchCount(htmlSource);

                if (matchCount > 1)
                    HandleMultipleMatches(item, htmlSource, matchCount, scraper);

                // Handle match count logic separately
                if (!ResultsFound(matchCount, item)) return;

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

                // Wait for the URL to change (indicating a page transition)
                wait.Until(d => d.Url != previousUrl);

                await GetParcelDataElements(scrapeBy, item, scraper);
            }
            catch (WebDriverException ex)
            {
                //CurrentItem = $"Web scraping error: {ex.Message}";
            }
            catch (Exception ex)
            {
                //CurrentItem = $"Unexpected error: {ex.Message}";
            }
        }

        public async Task GetParcelDataElements(ScrapeType _scrapeBy, ParcelData item, SeleniumWebDriverService scraper)
        {
            try
            {
                var originalParcelID = item.ParcelID;

                // Update the Regrid URL
                item.RegridUrl = scraper.WebDriver.Url;

                // Scrape the individual items
                if (_scrapeBy == ScrapeType.Address)
                    await UpdateElement(item, "ParcelID", true, scraper, "Parcel ID", "Parcel ID");

                // Get the address from Regrid in the event the address is already missing
                // We can have an address already from RealAuction and do not want to override 
                if (_scrapeBy == ScrapeType.Parcel && string.IsNullOrWhiteSpace(item.Address))
                    await UpdateElement(item, "Address", ShouldScrapeAddress, scraper, "Full Address", "Parcel Address");

                // Do not override the assessed value if already present
                if (!item.AssessedValue.HasValue)
                    await UpdateElement(item, "AssessedValue", ShouldScrapeAssessedValue, scraper, "Total Parcel Value",  "Assessed Value School District");

                await UpdateElement(item, "ZoningType", ShouldScrapeZoning, scraper, "Zoning type",  "Zoning Type", "Land Use");
                await UpdateElement(item, "ZoningCode", ShouldScrapeZoning, scraper, "Zoning Code",  "Parcel Use Code");
                await UpdateElement(item, "City", ShouldScrapeCity, scraper, "Parcel Address City", "Parcel Address City");
                await UpdateElement(item, "ZipCode", ShouldScrapeZipCode, scraper, "Parcel Address Zip Code",  "5 Digit Parcel Zip Code", "Zip Code");
                await UpdateElement(item, "Acres", ShouldScrapeAcres, scraper, "Measurements",  "Measurements");
                await UpdateElement(item, "OwnerName", ShouldScrapeOwner, scraper, "Owner",  "Enhanced Owner", "Owner Name (Assessor)");
                await UpdateElement(item, "GeographicCoordinate", ShouldScrapeCoordinates, scraper, "Centroid Coordinates",  "Centroid Coordinates");
                await UpdateElement(item, "FloodZone", ShouldScrapeFloodZone, scraper, "FEMA Flood Zone",  "FEMA NRI Risk Rating");
                await UpdateElement(item, "ParcelElevationHigh", true, scraper, "Highest Parcel Elevation", "Highest Parcel Elevation");
                await UpdateElement(item, "ParcelElevationLow", true, scraper, "Lowest Parcel Elevation", "Lowest Parcel Elevation");

                if (string.IsNullOrWhiteSpace(item.ParcelID))
                    item.ParcelID = originalParcelID;

                item.ScrapeStatus = ScrapeStatus.Complete;
            }
            catch (Exception ex)
            {

            }
        }

        private void HandleMultipleMatches(ParcelData item, string htmlSource, int matchCount, SeleniumWebDriverService scraper)
        {
            try
            {
                // Wait for the search results container to load
                var wait = new WebDriverWait(scraper.WebDriver, TimeSpan.FromSeconds(5));
                
                var divContainer = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("#pjax-container > div.container > div:nth-child(2) > div > div.search-results")));
                item.RegridSearchResults.Clear();
                var elements = divContainer.FindElements(By.CssSelector(".headline.parcel-result"));

                foreach (var element in elements)
                {
                    var anchorTag = element.FindElement(By.TagName("a"));
                    var url = anchorTag.GetAttribute("href");
                    var addressText = anchorTag.Text.Trim(); // Text inside the <a> tag

                    // Get the text right below the link
                    var cityText = "";
                    try
                    {
                        cityText = element.FindElement(By.XPath("following-sibling::div")).Text.Trim();
                    }
                    catch (NoSuchElementException)
                    {
                        cityText = ""; // Handle cases where text doesn't exist
                    }

                    if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(addressText))
                        item.RegridSearchResults.Add(new RegridSearchResult { ParcelAddress = addressText, ParcelCity = cityText, ParcelURL = url });
                }
            }
            catch (Exception ex)
            {
                //
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
                item.ZoningType = results == 0 ? $"Not found" : $"Multiple";
                item.ScrapeStatus = results == 0? ScrapeStatus.NotFound : ScrapeStatus.MultipleMatches;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Helper: Scrape property dynamically
        /// </summary>
        private async Task UpdateElement(ParcelData item, string propertyName, bool shouldScrape, SeleniumWebDriverService scraper, params string[] searchElements)
        {
            if (shouldScrape)
            {
                try
                {
                    var rawResult = await FindElement(scraper.WebDriver, searchElements);

                    // Remove dollar signs, commas, and trim whitespace
                    var cleanResult = rawResult?
                        .Replace("$", string.Empty)
                        .Replace(" : ", string.Empty)
                        .Trim();

                    item.SetPropertyValue(propertyName, cleanResult);
                }
                catch (Exception ex)
                {

                }
            }
        }

        /// <summary>
        /// Finds the text of the first available element among the specified search elements.
        /// </summary>
        /// </summary>
        /// <param name="searchElements">A collection of element selectors to search for.</param>
        /// <returns>The text of the first found element; otherwise, an empty string.</returns>
        private async Task<string> FindElement(IWebDriver driver, params string[] searchElements)
        {
            foreach (var element in searchElements)
            {
                var text = await FindElementContainer(driver, element).ConfigureAwait(false);
                
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