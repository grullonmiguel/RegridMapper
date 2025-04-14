using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using RegridMapper.Core.Commands;
using RegridMapper.Core.Configuration;
using RegridMapper.Core.Services;
using RegridMapper.Core.Utilities;
using RegridMapper.Services;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace RegridMapper.ViewModels
{
    public class ParcelViewModel : BaseViewModel
    {
        #region Fields

        private bool _cancelScraping; 
        private readonly Logger _logger;
        private readonly SeleniumScraper _scraper;
        private readonly OpenStreetMapService _openStreetMapService = new();

        #endregion

        #region Properties

        public ObservableCollection<ParcelData> ParcelList { get; } = [];

        public ObservableCollection<ParcelData> SelectedParcels { get; set; } = new();

        public bool CanPaste => ParcelList?.Count <= 0;

        public bool CanScrape => ParcelList.Count > 0;

        public bool IsScraping
        {
            get => _isScraping;
            set => SetProperty(ref _isScraping, value);
        }
        private bool _isScraping;

        public string RegridStatus
        {  
            get => _regridStatus; 
            set => SetProperty(ref _regridStatus, value); 
        }
        private string _regridStatus;

        public string CurrentScrapingElement
        {
            get => _currentScrapingElement;
            set => SetProperty(ref _currentScrapingElement, value);
        }
        private string _currentScrapingElement;

        public bool ParcelsSelected => SelectedParcels.Any();

        public string TotalParcels => ParcelList.Count <= 0 ? "" : $"Total Rows: {ParcelList.Count}";

        #endregion

        #region Commands

        public ICommand ClearDataCommand { get; }
        public ICommand SelectedParcelsCommand { get; }

        // Clipboard Commands
        public ICommand CopySelectedParcelsCommand => new RelayCommand(async () => await SaveToClipboard(), () => ParcelsSelected);
        public ICommand LoadFromClipboardCommand => new RelayCommand(async () => await LoadFromClipboard());

        // Regrid Scraping Commands
        public ICommand RegridQueryCancelCommand => new RelayCommand(async () => await CancelRegridScraping());
        public ICommand RegridQueryAllParcelsCommand => new RelayCommand(async () => await RegridQueryAllParcels());
        public ICommand RegridQuerySelectedCommand => new RelayCommand(async () => await RegridQuerySelectedParcels(), () => ParcelsSelected);

        // URL Navigation Commands
        public ICommand NavigateToFemaCommand => new RelayCommand<ParcelData>(item => UrlHelper.OpenUrl(item?.FemaUrl), OnCanNavigateFemaAddress);
        public ICommand NavigateToGoogleMapsCommand => new RelayCommand<ParcelData>(item => UrlHelper.OpenUrl(item?.GoogleUrl), OnCanNavigateGoogleMaps);
        public ICommand NavigateToRegridCommand => new RelayCommand<ParcelData>(item => UrlHelper.OpenUrl(item?.RegridUrl), OnCanNavigateRegrid);

        // OpenStreetMap Commands
        public ICommand OpenStreetQueryAllParcelsCommand => new RelayCommand(async () => await OpenStreetQueryAllParcels());
        public ICommand OpenStreetQuerySelectedParcelsCommand => new RelayCommand(async () => await OpenStreetQuerySelectedParcels(), () => ParcelsSelected);

        #endregion

        #region Constructor

        public ParcelViewModel()
        {
            _logger = Logger.Instance;
            ClearDataCommand = new RelayCommand(ClearData);
            SelectedParcelsCommand = new RelayCommand<IList>(OnSelectedParcelsChanged);
        } 

        #endregion

        #region Regrid Scraping

        private async Task RegridQueryAllParcels() 
            => await RegridScrapeParcels(ParcelList.ToList()); // Convert ObservableCollection to List

        private async Task RegridQuerySelectedParcels() 
            => await RegridScrapeParcels(SelectedParcels.ToList());

        private async Task RegridScrapeParcels(List<ParcelData> parcels)
        {
            if (parcels == null || parcels.Count == 0)
                return; // Avoid unnecessary execution if there are no parcels to process

            IsScraping = true; // Indicate process start
            OnPropertyChanged(nameof(IsScraping));

            try
            {
                await Task.Run(async () =>
                {
                    using var scraper = new SeleniumScraper(BrowserType.Chrome, true, "127.0.0.1:9222");
                    SemaphoreSlim semaphore = new(3);

                    for (int i = 0; i < parcels.Count; i++)
                    {
                        var item = parcels[i];
                        await semaphore.WaitAsync();

                        try
                        {
                            RegridStatus = $"Processing {i + 1} of {parcels.Count}.";

                            //var url = $"{AppConstants.BaseRegridUrlPrefix}{item.ParcelID}{AppConstants.BaseRegridUrlPostfix}";

                            CurrentScrapingElement = item?.ParcelID;

                            item.RegridUrl = $"{AppConstants.BaseRegridUrlPrefix}{item.ParcelID}{AppConstants.BaseRegridUrlPostfix}";
                            string pageSource = await Task.Run(() => scraper.ScrapeParcelData(item?.RegridUrl));

                            if (string.IsNullOrWhiteSpace(pageSource))
                            {
                                item.NoMatchDetected = true;
                                CurrentScrapingElement = $"NOT FOUND: {item?.ParcelID}";
                                await _logger.LogAsync($"Empty response for Parcel ID: {item.ParcelID}");
                                continue;
                            }

                            await ExtractParcelData(pageSource, item, scraper);
                        }
                        catch (WebDriverTimeoutException ex)
                        {
                            await _logger.LogExceptionAsync(ex);
                        }
                        catch (WebDriverException ex)
                        {
                            await _logger.LogExceptionAsync(ex);
                        }
                        catch (Exception ex)
                        {
                            await _logger.LogExceptionAsync(ex);
                        }
                        finally
                        {
                            semaphore.Release();
                            OnPropertyChanged(nameof(ParcelList));

                            // Add delay before moving to the next parcel
                            await Task.Delay(TimeSpan.FromSeconds(1));

                        }
                    }
                });
            }
            finally
            {
                IsScraping = false; // Indicate process end
                RegridStatus = "Completed!";
                CurrentScrapingElement = string.Empty;
                NotifyPropertiesChanged(nameof(IsScraping), nameof(RegridStatus));
            }
        }
        
        private int ExtractMatchCount(string pageSource)
        {
            Match match = Regex.Match(pageSource, @"Found (\d+) matches");
            return match.Success && int.TryParse(match.Groups[1].Value, out int count) ? count : -1;
        }

        private async Task ExtractParcelData(string pageSource, ParcelData item, SeleniumScraper scraper)
        {
            try
            {
                // Extract match count from page source
                var matchCount = ExtractMatchCount(pageSource);

                if (matchCount == 0)
                {
                    item.NoMatchDetected = true;
                    item.MultipleMatchesFound = false;
                    CurrentScrapingElement = $"No matches found - {item.ParcelID}";
                    await Task.Delay(1000);
                    return;
                }
                else if (matchCount > 1)
                {
                    item.NoMatchDetected = false;
                    item.MultipleMatchesFound = true;
                    CurrentScrapingElement = $"Multiple matches found - {item.ParcelID}";
                    await Task.Delay(1000);
                    return;
                }
                else if (matchCount == 1)
                {
                    await Task.Delay(500); // Allow page transition

                    var wait = new WebDriverWait(scraper.WebDriver, TimeSpan.FromSeconds(5));
                    var divContainer = wait.Until(d => scraper.FindElementSafely(By.CssSelector("div.headline.parcel-result")));

                    if (divContainer == null)
                    {
                        CurrentScrapingElement = $"Parcel result not found - {item.ParcelID}";
                        await Task.Delay(1000);
                        return;
                    }

                    // Find the Hyperlink and click on it to go to the parcel page
                    var hyperlink = divContainer.FindElement(By.TagName("a"));
                    hyperlink.Click();

                    wait = new WebDriverWait(scraper.WebDriver, TimeSpan.FromSeconds(1));

                    // ZONING
                    UpdateRegridStatusLabel(item, "Zoning Type");
                    item.ZoningType = await GetNextDivTextAsync(scraper.WebDriver, "Zoning Type");
                    if (string.IsNullOrEmpty(item.ZoningType)) item.ZoningType = await GetNextDivTextAsync(scraper.WebDriver, "Zoning type");
                    if (string.IsNullOrEmpty(item.ZoningType)) item.ZoningType = await GetNextDivTextAsync(scraper.WebDriver, "Zoning Description");
                    if (string.IsNullOrEmpty(item.ZoningType)) item.ZoningType = await GetNextDivTextAsync(scraper.WebDriver, "Land Use");

                    // CITY
                    UpdateRegridStatusLabel(item, "City");
                    item.City = await GetNextDivTextAsync(scraper.WebDriver, "Parcel Address City");

                    // ZIP CODE
                    UpdateRegridStatusLabel(item, "Zip Code");
                    item.ZipCode = await GetNextDivTextAsync(scraper.WebDriver, "5 Digit Parcel Zip Code");
                    if (string.IsNullOrEmpty(item.ZipCode)) item.ZipCode = await GetNextDivTextAsync(scraper.WebDriver, "Zip Code");

                    // REGRID
                    item.RegridUrl = scraper.WebDriver.Url;

                    // ADDRESS
                    UpdateRegridStatusLabel(item, "Address");
                    item.Address = await GetNextDivTextAsync(scraper.WebDriver, "Full Address");

                    // OWNER
                    UpdateRegridStatusLabel(item, "Owner Name");
                    item.OwnerName = await GetNextDivTextAsync(scraper.WebDriver, "Owner");
                    if (string.IsNullOrEmpty(item.Acres)) item.OwnerName = await GetNextDivTextAsync(scraper.WebDriver, "Owner");

                    // ASSESSED
                    UpdateRegridStatusLabel(item, "Assessed Value");
                    item.AssessedValue = await GetNextDivTextAsync(scraper.WebDriver, "Total Parcel Value");

                    // ACRES
                    UpdateRegridStatusLabel(item, "Acres");
                    item.Acres = await GetNextDivTextAsync(scraper.WebDriver, "Measurements");
                    if (string.IsNullOrEmpty(item.Acres)) item.Acres = await GetNextDivTextAsync(scraper.WebDriver, "Measurements");
                    if (string.IsNullOrEmpty(item.Acres)) item.Acres = await GetNextDivTextAsync(scraper.WebDriver, "Measurements");
                    if (!string.IsNullOrEmpty(item.Acres)) item.Acres = item.Acres.ToLower().Replace(" acres", "");

                    // COORDINATE
                    UpdateRegridStatusLabel(item, "Latitude / Longitude");
                    item.GeographicCoordinate = await GetNextDivTextAsync(scraper.WebDriver, "Centroid Coordinates");

                    // FLOOD ZONE
                    UpdateRegridStatusLabel(item, "FEMA Flood Zone");
                    item.FloodZone = await GetNextDivTextAsync(scraper.WebDriver, "FEMA Flood Zone");
                    if (string.IsNullOrEmpty(item.FloodZone)) item.FloodZone = await GetNextDivTextAsync(scraper.WebDriver, "FEMA NRI Risk Rating");
                    if (string.IsNullOrEmpty(item.FloodZone))  item.FloodZone = await GetNextDivTextAsync(scraper.WebDriver, "N/A");
                }
                else
                {
                    //item.Response = "Match count could not be determined";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> GetNextDivTextAsync(IWebDriver driver, string searchText, int maxRetries = 5)
        {
            IWebElement target = null;
            IWebElement nextDiv = null;

            try
            {
                int retryCount = 0;
                while (target == null && retryCount < maxRetries)
                {
                    target = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript(
                        $"return document.evaluate(\"//div[contains(text(), '{searchText}')]\"," +
                        "document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;");

                    if (target == null)
                    {
                        await Task.Delay(500); // Asynchronous wait before retrying
                        retryCount++;
                    }
                }
            }
            catch
            {
                return $"";
            }

            // Locate the next sibling div
            if (target != null)
            {
                try
                {
                    nextDiv = target.FindElement(By.XPath("following-sibling::div"));
                }
                catch
                { }

                // Look inside nested span
                if (nextDiv == null)
                {
                    try
                    {
                        nextDiv = target.FindElement(By.XPath("ancestor::div/following-sibling::div[contains(@class, 'field-value')]//span"));
                    }
                    catch
                    { }
                }
            }

            var returnVal = nextDiv != null ?
                nextDiv.Text.Replace("\r\n", " ") :
                (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0] ? arguments[0].closest('.field')?.querySelector('.field-value span')?.innerText || 'Not found' : '';", target);

            return returnVal;
        }

        private Task CancelRegridScraping()
        {
            _cancelScraping = true;
            return Task.CompletedTask;
        }

        #endregion

        #region OpenStreetMap

        private async Task OpenStreetQueryAllParcels()
            => await OpenStreetScrapeParcels(ParcelList.ToList()); // Convert ObservableCollection to List

        private async Task OpenStreetQuerySelectedParcels()
            => await OpenStreetScrapeParcels(SelectedParcels.ToList());

        private async Task OpenStreetScrapeParcels(List<ParcelData> parcels)
        {
            foreach (var parcel in parcels)
            {
                // Ensure coordinates are not null or empty before making the request
                if (!string.IsNullOrWhiteSpace(parcel.GeographicCoordinate))
                {
                    var response = await _openStreetMapService.GetLocationDataAsync(parcel.GeographicCoordinate);

                    // Optionally store the response inside the parcel object
                    //parcel.LocationData = response;
                }
                else
                {
                    Console.WriteLine($"Skipping parcel with empty coordinates.");
                }
            }
        }

        #endregion

        #region Clipboard

        private async Task LoadFromClipboard()
        {
            try
            {
                string clipboardText = Clipboard.GetText();
                var parcels = clipboardText.Split('\n')
                                           .Select(p => p.Trim()) // Trim early
                                           .Where(p => !string.IsNullOrEmpty(p)) // Filter empty entries
                                           .Select(p => new ParcelData { ParcelID = p }) // Map directly
                                           .ToList(); // Convert to list

                ParcelList.Clear();
                ParcelList.AddRange(parcels); // More efficient than adding one by one

                await _logger.LogAsync($"Loaded {ParcelList.Count} parcels from clipboard.");
            }
            catch (Exception ex)
            {
                await _logger.LogExceptionAsync(ex);
            }
            finally
            {
                NotifyPropertiesChanged(nameof(CanPaste), nameof(CanScrape), nameof(TotalParcels));
            }
        }

        private async Task SaveToClipboard()

        {
            if (SelectedParcels == null || !SelectedParcels.Any())
                return; // Exit if no parcels are selected

            var clipboardText = new StringBuilder();

            // Add the headers
            clipboardText.AppendLine($"Type\t" + 
                                     $"County\t" +
                                     $"City\t" +
                                     $"Parcel ID\t" +
                                     $"Regrid\t" +
                                     $"Address\t" +
                                     $"Owner\t" +
                                     $"Appraisal\t" +
                                     $"Assessed Value\t" +
                                     $"Acres\t" +
                                     $"Maps\t" +
                                     $"Fema");

            foreach (var item in SelectedParcels)
            {
                // Structure the hyperlinl with an alias for Google Sheets or Excel
                string regrid = !string.IsNullOrWhiteSpace(item.RegridUrl) ?
                    $"=HYPERLINK(\"{item.RegridUrl}\", \"LINK\")" : "";

                string maps = !string.IsNullOrWhiteSpace(item.GoogleUrl)
                    ? $"=HYPERLINK(\"{item.GoogleUrl.Replace("\"", "\"\"")}\", \"LINK\")" : "";

                string fema = !string.IsNullOrWhiteSpace(item.FemaUrl)
                    ? $"=HYPERLINK(\"{item.FemaUrl.Replace("\"", "\"\"")}\", \"{item.FloodZone}\")" : "";

                clipboardText.AppendLine($"{item.ZoningType}\t" +
                                         $"{item.County}\t" +
                                         $"{item.City}\t" +
                                         $"{item.ParcelID}\t" +
                                         $"{regrid}\t" +
                                         $"{item.Address}\t" +
                                         $"{item.OwnerName}\t" +
                                         $"\t" +
                                         $"{item.AssessedValue}\t" +
                                         $"{item.Acres}\t" +
                                         $"{maps}\t" +
                                         $"{fema}\t");
            }

            // Runs clipboard operation on the UI thread, preventing STA errors.

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Clipboard.SetText(clipboardText.ToString());
            });

        }

        #endregion

        #region Helpers

        private void ClearData()
        {
            ParcelList.Clear();
            RegridStatus = string.Empty;
            NotifyPropertiesChanged(nameof(CanPaste), nameof(CanScrape), nameof(TotalParcels));
        }

        private void OnSelectedParcelsChanged(IList? selectedItems)
        {
            if (selectedItems == null)
                return; // Avoid null reference errors

            // Use HashSet for fast lookups to prevent duplicate entries
            var newSelection = new HashSet<ParcelData>(selectedItems.Cast<ParcelData>());

            // Remove items that are no longer selected
            SelectedParcels = new ObservableCollection<ParcelData>(
                SelectedParcels.Where(parcel => newSelection.Contains(parcel))
            );

            // Add newly selected items
            foreach (var parcel in newSelection)
            {
                if (!SelectedParcels.Contains(parcel)) // Prevent duplicates
                    SelectedParcels.Add(parcel);
            }

            OnPropertyChanged(nameof(SelectedParcels));
            OnPropertyChanged(nameof(ParcelsSelected));
        }

        private void NotifyPropertiesChanged(params string[] propertyNames)
        {
            foreach (var property in propertyNames)
                OnPropertyChanged(property);
        }

        private bool OnCanNavigateFemaAddress(ParcelData item)
            => item != null && UrlHelper.IsValidUrl(item?.FemaUrl);

        private bool OnCanNavigateGoogleMaps(ParcelData item)
            => item != null && UrlHelper.IsValidUrl(item?.GoogleUrl);

        private bool OnCanNavigateRegrid(ParcelData item)
            => item != null && UrlHelper.IsValidUrl(item?.RegridUrl);

        private void UpdateRegridStatusLabel(ParcelData item, string message) 
            => CurrentScrapingElement = $"Scraping {message} for Parcel {item.ParcelID}";

        #endregion
    }
}