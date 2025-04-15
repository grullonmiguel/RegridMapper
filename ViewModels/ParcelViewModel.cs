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
        public ICommand NavigateAppraiserCommand => CreateNavigateCommand(item => item?.AppraiserUrl);
        public ICommand NavigateDetailsCommand => CreateNavigateCommand(item => item?.DetailUrl);
        public ICommand NavigateToFemaCommand => CreateNavigateCommand(item => item?.FemaUrl);
        public ICommand NavigateToGoogleMapsCommand => CreateNavigateCommand(item => item?.GoogleUrl);
        public ICommand NavigateToRegridCommand => CreateNavigateCommand(item => item?.RegridUrl);
        public ICommand NavigateToRealtorCommand => CreateNavigateCommand(item => item?.RealtorUrl);
        public ICommand NavigateToRedfinCommand => CreateNavigateCommand(item => item?.RedfinUrl);
        public ICommand NavigateToZillowCommand => CreateNavigateCommand(item => item?.ZillowUrl);
        public ICommand CreateNavigateCommand(Func<ParcelData, string> urlSelector) => new RelayCommand<ParcelData>(item => UrlHelper.OpenUrl(urlSelector(item)), item => item != null && UrlHelper.IsValidUrl(urlSelector(item)));

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

                            item.RegridUrl = string.Format(AppConstants.URL_Regrid, Uri.EscapeDataString(item.ParcelID));

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
                    UpdateRegridStatusLabel(item, AppConstants.RegridZoningType);
                    item.ZoningType = await GetNextDivTextAsync(scraper.WebDriver, AppConstants.RegridZoningType);
                    if (string.IsNullOrEmpty(item.ZoningType)) item.ZoningType = await GetNextDivTextAsync(scraper.WebDriver, "Zoning type");
                    if (string.IsNullOrEmpty(item.ZoningType)) item.ZoningType = await GetNextDivTextAsync(scraper.WebDriver, "Zoning Description");
                    if (string.IsNullOrEmpty(item.ZoningType)) item.ZoningType = await GetNextDivTextAsync(scraper.WebDriver, "Land Use");

                    // CITY
                    UpdateRegridStatusLabel(item, AppConstants.RegridCity);
                    item.City = await GetNextDivTextAsync(scraper.WebDriver, AppConstants.RegridCity);

                    // ZIP CODE
                    UpdateRegridStatusLabel(item, AppConstants.RegridZip);
                    item.ZipCode = await GetNextDivTextAsync(scraper.WebDriver, AppConstants.RegridZip);
                    if (string.IsNullOrEmpty(item.ZipCode)) item.ZipCode = await GetNextDivTextAsync(scraper.WebDriver, AppConstants.RegridZip2);

                    // REGRID
                    item.RegridUrl = scraper.WebDriver.Url;

                    // ADDRESS
                    UpdateRegridStatusLabel(item, AppConstants.RegridAddress);
                    item.Address = await GetNextDivTextAsync(scraper.WebDriver, AppConstants.RegridAddress);

                    // OWNER
                    UpdateRegridStatusLabel(item, AppConstants.RegridOwner);
                    item.OwnerName = await GetNextDivTextAsync(scraper.WebDriver, AppConstants.RegridOwner);
                    if (string.IsNullOrEmpty(item.Acres)) item.OwnerName = await GetNextDivTextAsync(scraper.WebDriver, AppConstants.RegridOwner);

                    // ASSESSED
                    UpdateRegridStatusLabel(item, AppConstants.RegridAssessedValue);
                    item.AssessedValue = await GetNextDivTextAsync(scraper.WebDriver, AppConstants.RegridAssessedValue);

                    // ACRES
                    UpdateRegridStatusLabel(item, AppConstants.RegridAcres);
                    item.Acres = await GetNextDivTextAsync(scraper.WebDriver, AppConstants.RegridAcres);
                    if (string.IsNullOrEmpty(item.Acres)) item.Acres = await GetNextDivTextAsync(scraper.WebDriver, AppConstants.RegridAcres);
                    if (!string.IsNullOrEmpty(item.Acres)) item.Acres = item.Acres.ToLower().Replace(" acres", "");

                    // COORDINATE
                    UpdateRegridStatusLabel(item, AppConstants.RegridCoordinates);
                    item.GeographicCoordinate = await GetNextDivTextAsync(scraper.WebDriver, AppConstants.RegridCoordinates);

                    // FLOOD ZONE
                    UpdateRegridStatusLabel(item, AppConstants.RegridFema);
                    item.FloodZone = await GetNextDivTextAsync(scraper.WebDriver, AppConstants.RegridFema);
                    if (string.IsNullOrEmpty(item.FloodZone)) item.FloodZone = await GetNextDivTextAsync(scraper.WebDriver, AppConstants.RegridFema2);
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
                                     $"Fema\t" +
                                     $"Realtor\t" +
                                     $"Redfin\t" +
                                     $"Zillow\t");

            foreach (var item in SelectedParcels)
            {
                // Structure the hyperlinl with an alias for Google Sheets or Excel
                var regridURL = string.IsNullOrWhiteSpace(item.RegridUrl) ? "" : string.Format(AppConstants.HYPERLINK_FORMAT, item.RegridUrl, "LINK");
                var mapsURL =   string.IsNullOrWhiteSpace(item.GoogleUrl) ? "" : string.Format(AppConstants.HYPERLINK_FORMAT, item.GoogleUrl.Replace("\"", "\"\""), "LINK");
                var femaURL =   string.IsNullOrWhiteSpace(item.FemaUrl)   ? "" : string.Format(AppConstants.HYPERLINK_FORMAT, item.FemaUrl.Replace("\"", "\"\""), item.FloodZone);
                var realtorURL =   string.IsNullOrWhiteSpace(item.RealtorUrl)   ? "" : string.Format(AppConstants.HYPERLINK_FORMAT, item.RealtorUrl.Replace("\"", "\"\""), "LINK");
                var redfinURL =   string.IsNullOrWhiteSpace(item.RedfinUrl)   ? "" : string.Format(AppConstants.HYPERLINK_FORMAT, item.RedfinUrl.Replace("\"", "\"\""), "LINK");
                var zillowURL =   string.IsNullOrWhiteSpace(item.ZillowUrl)   ? "" : string.Format(AppConstants.HYPERLINK_FORMAT, item.ZillowUrl.Replace("\"", "\"\""), "LINK");

                clipboardText.AppendLine($"{item.ZoningType}\t" +
                                         $"{item.County}\t" +
                                         $"{item.City}\t" +
                                         $"{item.ParcelID}\t" +
                                         $"{regridURL}\t" +
                                         $"{item.Address}\t" +
                                         $"{item.OwnerName}\t" +
                                         $"LINK\t" +
                                         $"{item.AssessedValue}\t" +
                                         $"{item.Acres}\t" +
                                         $"{mapsURL}\t" +
                                         $"{femaURL}\t" +
                                         $"{realtorURL}\t" +
                                         $"{redfinURL}\t" +
                                         $"{zillowURL}\t");
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

        private bool OnCanNavigateFemaAddress(ParcelData item) => item != null && UrlHelper.IsValidUrl(item?.FemaUrl);

        private bool OnCanNavigateGoogleMaps(ParcelData item) => item != null && UrlHelper.IsValidUrl(item?.GoogleUrl);

        private bool OnCanNavigateRegrid(ParcelData item) => item != null && UrlHelper.IsValidUrl(item?.RegridUrl);

        private bool OnCanNavigateRealtor(ParcelData item) => item != null && UrlHelper.IsValidUrl(item?.RealtorUrl);

        private bool OnCanNavigateRedfin(ParcelData item) => item != null && UrlHelper.IsValidUrl(item?.RedfinUrl);

        private bool OnCanNavigateZillow(ParcelData item) => item != null && UrlHelper.IsValidUrl(item?.ZillowUrl);

        //private bool OnCanNavigateDetails(ParcelData item) => item != null && UrlHelper.IsValidUrl(item?.DetailsUrl);

        private void UpdateRegridStatusLabel(ParcelData item, string message) 
            => CurrentScrapingElement = $"Scraping {message} for Parcel {item.ParcelID}";

        #endregion
    }
}