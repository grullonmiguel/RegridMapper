using OpenQA.Selenium;
using RegridMapper.Core.Commands;
using RegridMapper.Core.Configuration;
using RegridMapper.Core.Services;
using RegridMapper.Core.Utilities;
using RegridMapper.Models;
using RegridMapper.Services;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace RegridMapper.ViewModels
{
    public class RealAuctionViewModel : BaseViewModel
    {
        #region Fields

        private string? _preformattedAuctionUrl;
        private string? _preformattedAppraiserUrl;
        private readonly StateDataService _dataService;

        #endregion

        #region Commands
        
        public ICommand AuctionUrlSaveCommand => new RelayCommand(async ()=> await SaveAuctionUrl(), ()=> StateSelected != null && CountySelected != null && AuctionDate.HasValue);

        // Clipboard Commands
        public ICommand CopyParcelsCommand => new RelayCommand(async ()=> await SaveToClipboard(), ()=> ParcelList.Any());

        // Real Auction Scraping Commands
        public ICommand ScrapeRealAuctionCommand => new RelayCommand(async ()=> await ScrapeRealAuctionList(), ()=> CanScrapeRealAuction);

        // Regrid Commands
        public ICommand ScrapeRegridByAddressCommand => new RelayCommand(async ()=> await ScrapeRegridParcels(ScrapeType.Address), () => ParcelList.Any() && !IsScraping);
        public ICommand ScrapeRegridByParcelIDCommand => new RelayCommand(async ()=> await ScrapeRegridParcels(ScrapeType.Parcel), () => ParcelList.Any() && !IsScraping);
  
        // URL Navigation Commands
        public ICommand NavigateToAuctionUrlCommand => new RelayCommand(() => NavigateToAuctionUrl());
        public ICommand NavigateToCountyUrlCommand => RealAuctionNavigateCommand(item => item?.RealAuctionURL);
        public ICommand RealAuctionNavigateCommand(Func<US_County, string> urlSelector) => new RelayCommand<US_County>(item => UrlHelper.OpenUrl(urlSelector(item)), item => item != null && UrlHelper.IsValidUrl(urlSelector(item)));

        #endregion

        #region Properties 

        public bool RegridColumnsVisible
        {
            get => _regridColumnsVisible;
            set => SetProperty(ref _regridColumnsVisible, value);
        }
        private bool _regridColumnsVisible = false;

        public US_State? StateSelected
        {
            get => _stateSelected;
            set
            {
                Counties = null;
                SetProperty(ref _stateSelected, value);
                Counties = StateSelected?.Counties;
            }
        }
        private US_State? _stateSelected;

        public US_County? CountySelected
        {
            get => _countySelected;
            set
            {
                if (_countySelected !=  value)
                {
                    SetProperty(ref _countySelected, value);
                    AuctionCounty = value?.Name ?? string.Empty;
                    _preformattedAuctionUrl = value?.AuctionUrl ?? string.Empty;
                    _preformattedAppraiserUrl = value?.AppraiserUrl ?? string.Empty;
                    UpdateAuctionUrl();
                    OnPropertyChanged(nameof(CanScrape));
                }

            }
        }
        private US_County? _countySelected;

        public IEnumerable<US_State>? States
        {
            get => _states;
            set => SetProperty(ref _states, value);
        }
        private IEnumerable<US_State>? _states;

        public List<US_County>? Counties
        {
            get => _counties;
            set => SetProperty(ref _counties, value);
        }
        private List<US_County>? _counties;
        
        public bool CanScrapeRealAuction => !string.IsNullOrWhiteSpace(AuctionURL) && !IsScraping;

        public DateTime? AuctionDate
        {
            get => _auctionDate;
            set
            {
                if (_auctionDate != value)
                {
                    SetProperty(ref _auctionDate, value);
                    UpdateAuctionUrl();
                    OnPropertyChanged(nameof(CanScrape));
                }
            }
        }
        private DateTime? _auctionDate;

        public string AuctionURL
        {
            get => _auctionURL;
            set => SetProperty(ref _auctionURL, value);
        }
        private string _auctionURL;

        public string AuctionCounty 
        { 
            get => _auctionCounty; 
            set => SetProperty(ref _auctionCounty, value); 
        }
        private string _auctionCounty;

        #endregion

        #region Constructor

        public RealAuctionViewModel()
        {
            _dataService = new StateDataService();
            InitializeAsync();
            SettingsOpenCommand = new RelayCommand(() => ShowCountyEditDialog());
            SettingsCloseCommand = new RelayCommand(() => CloseCountyEditDialog());
        }

        private async void InitializeAsync()
        {
            await GetStateList();
            await LoadSettings();
        }

        #endregion

        #region Methods

        private async Task ScrapeRealAuctionList()
        {
            if (!CanScrapeRealAuction || IsScraping)
                return;

            ParcelList.Clear();
            var realAuctionList = new List<string>();

            // Indicate process start
            IsScraping = true;

            RegridColumnsVisible = false;

            // Record the start time
            var startTime = DateTime.Now;
            var isLastRecord = false;

            try
            {
                await Task.Run(async () =>
                {
                    // Connect to a chrome session with debugging enabled
                    using var scraper = new SeleniumWebDriverService(BrowserType.Chrome, false);

                    // Get the HTML from the Auction URL property
                    await scraper.NavigateToUrl(AuctionURL);

                    while (!isLastRecord)
                    {
                        var newData = await scraper.CaptureHTMLTable();

                        foreach (var item in newData)
                        {
                            // Stop pagination if duplicate found
                            if (realAuctionList.Contains(item))
                            {
                                Console.WriteLine("Item already exists! Stopping pagination.");
                                isLastRecord = true;
                                break;
                            }

                            realAuctionList.Add(item);
                        }

                        // Attempt to click the Next Page button
                        if (!scraper.TryClickNextPage())
                        {
                            Console.WriteLine("Next page button not found. Stopping pagination.");
                            isLastRecord = true;
                        }

                        // Wait briefly between pages to stabilize interactions
                        await Task.Delay(300);
                    }

                    // Consolidate all rows into a structured list
                    ProcessAuctionData(realAuctionList);
                });
            }
            finally
            {
                // Display elapsed time in minutes and seconds
                var elapsedTime = DateTime.Now - startTime;

                IsScraping = false; // Indicate process end
                OnPropertyChanged(nameof(TotalParcels));
            }
        }

        private void ProcessAuctionData(List<string> list)
        {
            // Extract raw auction data
            var waitingList = ExtractAuctionData("Auction Starts", "Auction Status", list);

            // Assign properties to auction items based on extracted data
            GetAuctionProperties(waitingList, "Auction Starts");

            // Process matching parcels for each auction item
            //Parallel.ForEach(ParcelList, item =>
            //{
            //    item.MatchingParcels = GetMatchingParcels(item.ParcelID, AuctionItems.ToList());
            //});
        }

        private List<string> ExtractAuctionData(string keep, string remove, List<string> list)
        {
            bool keepData = false;
            var filteredList = new List<string>();

            foreach (var data in list)
            {
                foreach (var line in data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.Contains(keep))
                        keepData = true;

                    else if (line.Contains(remove))
                        keepData = false;

                    else if (line.ToLower().Contains("user name"))
                        keepData = false;

                    if (keepData)
                        filteredList.Add(line.Trim());
                }
            }   

            return filteredList;
        }

        private async Task GetAuctionProperties(List<string> list, string keyword)
        {
            ParcelData currentItem = null;
            List<ParcelData> parcelsToAdd = new List<ParcelData>();

            HashSet<string> keywords = new HashSet<string>
            {
                "Case",
                "Opening",
                "Parcel",
                "Assessed",
                "page of",
                "Auction"
            };

            try
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        string line = list[i];

                        if (line == keyword) // Detect new auction start
                        {
                            if (currentItem != null)
                            {
                                parcelsToAdd.Add(currentItem); // Batch items for UI update
                            }

                            currentItem = new ParcelData(); // Initialize a new object
                            continue;
                        }

                        if (currentItem != null)
                        {
                            ReadOnlySpan<char> span = line.AsSpan();

                            switch (line)
                            {
                                case string s when span.StartsWith("Case #:"):
                                    currentItem.DetailUrl = span.Slice("Case #: ".Length).Trim().ToString();
                                    break;

                                case string s when span.StartsWith("Opening Bid:"):
                                    currentItem.AskingPrice = decimal.TryParse(span.Slice("Opening Bid: $".Length).Trim().ToString(), out var bid) ? bid : 0;
                                    break;

                                case string s when span.StartsWith("Parcel ID:") || span.StartsWith("Alternate Key:"):

                                    var prefix = span.StartsWith("Parcel ID:") ? "Parcel ID: " : "Alternate Key: ";
                                    currentItem.ParcelID = span.Slice(prefix.Length).Trim().ToString();

                                    if (!string.IsNullOrEmpty(currentItem.ParcelID))
                                    {
                                        // Modify Parcel ID for certain counties
                                        var parcel = currentItem.ParcelID;
                                        if(AuctionCounty == "Miami Dade")
                                            parcel = currentItem.ParcelID.Replace("-", "");
                                        
                                        // Appraiser's office web link
                                        currentItem.AppraiserUrl = string.Format(_preformattedAppraiserUrl, Uri.EscapeDataString(parcel));

                                        // Regrid web link
                                        if(prefix.StartsWith("Parcel ID:"))
                                            currentItem.RegridUrl = string.Format(AppConstants.URL_Regrid, Uri.EscapeDataString(currentItem.ParcelID));
                                    }

                                    break;

                                case string s when span.StartsWith("Property Address:"):
                                    currentItem.Address = span.Slice("Property Address: ".Length).Trim().ToString();

                                    // Look ahead and use LINQ for filtering
                                    string nextLine = i + 1 < list.Count ? list[i + 1].Trim() : null;
                                    if (!string.IsNullOrEmpty(nextLine) && !keywords.Any(nextLine.StartsWith))
                                    {
                                        currentItem.Address += $", {nextLine}";
                                        //i++; // Skip next line
                                    }
                                    break;

                                case string s when span.StartsWith("Assessed Value:"):
                                    var val = span.Slice("Assessed Value: $".Length).Trim().ToString();

                                    if (decimal.TryParse(val, out decimal assessedValue))
                                        currentItem.AssessedValue = assessedValue;

                                    break;
                            }
                        }
                    }

                    // Add last auction item
                    if (currentItem != null)
                    {
                        parcelsToAdd.Add(currentItem);
                    }
                });

                // Batch update UI efficiently
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ParcelList.AddRange(parcelsToAdd);
                });
            }
            catch (Exception ex)
            {
                //TODO: Debug.WriteLine($"Error processing auction properties: {ex.Message}");
            }
        }

        private async Task GetStateList()
        {
            States = await _dataService.GetAllStates() as List<US_State>;
            //if (States != null && States?.ToList().Count > 0)
            //{
            //    StateSelected = States.ToList()[0];
            //}
        }
       
        private void CloseCountyEditDialog()
        {
            ShowSettings = false;
            LoadSettings();
        }

        private void ShowCountyEditDialog()
        {
            ShowSettings = true;
        }

        private void NavigateToAuctionUrl()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AuctionURL))
                    return;

                int splitIndex = AuctionURL.IndexOf(".cfm");
                if (splitIndex == -1) // Ensure ".cfm" exists
                {
                    UrlHelper.OpenUrl(AuctionURL);
                    return;
                }

                splitIndex += 4; // Include ".cfm"
                string firstPart = AuctionURL.Substring(0, splitIndex);
                string secondPart = AuctionURL.Substring(splitIndex);

                UrlHelper.OpenUrl(secondPart.Contains("{0}") ? firstPart : AuctionURL);
            }
            catch (Exception ex)
            {
                // Log error instead of raw throw to prevent crashing
                Console.WriteLine($"Error navigating to URL: {ex.Message}");
            }
        }

        private void UpdateAuctionUrl()
        {
            if (string.IsNullOrWhiteSpace(_preformattedAuctionUrl) || AuctionDate is null)
                return;

            AuctionURL = $"{_preformattedAuctionUrl.Replace("{0}", AuctionDate?.ToString("MM/dd/yyyy"))}";
        }

        private async Task SaveToClipboard()
        {
            if (ParcelList is null || !ParcelList.Any())
                return; // Exit if no parcels are selected

            var clipboardText = new StringBuilder();

            // Define Headers
            string[] headers = { "PARCEL ID / APPRAISER", "REGRID", "ADDRESS", "ASSESSED VALUE", "OPENING BID"};

            // Append headers
            clipboardText.AppendLine(string.Join("\t", headers));

            // Helper method to format hyperlinks correctly for Google Sheets
            static string FormatUrl(string url, string alias) => string.IsNullOrWhiteSpace(url) ? string.Empty : $"=HYPERLINK(\"{url.Replace("\"", "\"\"")}\", \"{alias}\")";

            foreach (var item in ParcelList)
            {
                // Generate hyperlinks with correct spreadsheet formatting
                var urls = new[]
                {
                    FormatUrl(item.AppraiserUrl, item.ParcelID),
                    FormatUrl(item.RegridUrl, "LINK"),
                };

                // Create row data while ensuring Excel formatting compatibility
                string[] row =
                {
                    urls[0], urls[1], item.Address, item.AssessedValue.ToString(), item.AskingPrice.ToString()
                };

                clipboardText.AppendLine(string.Join("\t", row));
            }

            // Clipboard operation runs on UI thread
            await Application.Current.Dispatcher.InvokeAsync(() => Clipboard.SetText(clipboardText.ToString()));
        }

        #endregion

        #region Regrid

        protected override async Task ScrapeParcelWithMultipleMaches(string url)
        {
            _cancellationTokenSource?.Cancel(); // Cancel any previous operation
            _cancellationTokenSource = new CancellationTokenSource();
            await ScrapeRegrid(SelectedParcels.ToList(), _cancellationTokenSource.Token, url);
        }

        private async Task ScrapeRegridParcels(ScrapeType scrapeBy)
        {
            if (!CanScrape || IsScraping)
                return;

            _scrapeBy = scrapeBy;
            _cancellationTokenSource?.Cancel(); // Cancel any previous operation
            _cancellationTokenSource = new CancellationTokenSource();
            await ScrapeRegrid(ParcelList.ToList(), _cancellationTokenSource.Token, string.Empty);
        }

        private async Task ScrapeRegrid(List<ParcelData> parcels, CancellationToken cancellationToken, string url = "")
        {
            await Task.CompletedTask;

            if (!CanScrape || IsScraping)
                return;

            // Indicate process start
            IsScraping = true;

            // Record the start time
            var startTime = DateTime.Now;

            try
            {
                await Task.Run(async () =>
                {
                    // Connect to a chrome session with debugging enabled
                    using var scraper = new SeleniumWebDriverService("user_name", "password!");

                    RegridColumnsVisible = true;

                    for (int i = 0; i < parcels.Count; i++)
                    {
                        // Check if cancellation is requested and exit early if so
                        if (cancellationToken.IsCancellationRequested)
                        {
                            Status = "Scraping process canceled.";
                            return;
                        }

                        var item = parcels[i];

                        try
                        {
                            Status = $"Processing {i + 1} of {parcels.Count}.";
                            CurrentItem = _scrapeBy == ScrapeType.Parcel ? item?.ParcelID : item?.Address;

                            // Set initial Regrid URL
                            item!.RegridUrl = 
                                !string.IsNullOrWhiteSpace(url) ? url : string.Format(AppConstants.URL_Regrid, Uri.EscapeDataString(CurrentItem));

                            // Get the HTML for the selected Parcel ID
                            var htmlSource = await scraper.CaptureHTMLSource(item!.RegridUrl);

                            // Verify something was scraped
                            if (string.IsNullOrWhiteSpace(htmlSource))
                            {
                                item.ScrapeStatus = ScrapeStatus.NotFound;
                                Status = $"NOT FOUND: {item?.ParcelID}";
                                await _logger!.LogAsync($"Empty response for Parcel ID: {CurrentItem}");
                                continue;
                            }

                            if(string.IsNullOrWhiteSpace(url))
                                await _regriDataService.GetParcelData(htmlSource, item, scraper);
                            else
                                await _regriDataService.GetParcelDataElements(item, scraper);
                        }
                        catch (WebDriverTimeoutException ex) { await _logger!.LogExceptionAsync(ex); }
                        catch (WebDriverException ex)        { await _logger!.LogExceptionAsync(ex); }
                        catch (Exception ex)                 { await _logger!.LogExceptionAsync(ex); }
                        finally
                        {
                            NotifyPropertiesChanged(nameof(IsScraping), nameof(Status), nameof(CanScrape), nameof(SelectedParcels), nameof(ParcelList));
                        }
                    };
                });
            }
            finally
            {
                // Display elapsed time in minutes and seconds
                var elapsedTime = DateTime.Now - startTime;
                Status = $"Completed in {elapsedTime.Minutes} minutes and {elapsedTime.Seconds} seconds";
 
                // Indicate process end
                IsScraping = false;
                CurrentItem = string.Empty;
                NotifyPropertiesChanged(nameof(IsScraping), nameof(Status), nameof(CanScrape), nameof(SelectedParcels), nameof(ParcelList));
            }
        }

        #endregion

        #region Settings

        private async Task LoadSettings()
        {
            await Task.CompletedTask;
            try
            {
                AuctionURL = SettingsService.LoadSetting<string>("AuctionURL");
                AuctionCounty = SettingsService.LoadSetting<string>("AuctionCounty");
                _preformattedAppraiserUrl = SettingsService.LoadSetting<string>("AppraiserURL");
                OnPropertyChanged(nameof(CanScrape));
            }
            catch (Exception)
            {
                // Provide recovery action
            }
        }

        private async Task SaveSettings(string url, string settingKey)
        {
            await Task.CompletedTask;
            try
            {
                // Allow blanks but do not allow invalid URLs
                if (!string.IsNullOrEmpty(url) && !UrlHelper.IsValidUrl(url))
                    return;

                SettingsService.SaveSetting(settingKey, url);
            }
            catch (Exception)
            {
                // Provide recovery action
            }
        }

        private async Task SaveAuctionUrl()
        {
            await SaveSettings(AuctionURL, "AuctionUrl");
            await SaveSettings(_preformattedAppraiserUrl, "AppraiserUrl");
            SettingsService.SaveSetting("AuctionCounty", AuctionCounty);

            CloseCountyEditDialog();
        }

        #endregion
    }
}