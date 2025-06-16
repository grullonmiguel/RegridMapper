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

        // Real Auction Scraping Commands
        public ICommand ScrapeRealAuctionCommand => new RelayCommand(async ()=> await GetRealAuctionHTML(), ()=> CanScrapeRealAuction);
  
        // URL Navigation Commands
        public ICommand NavigateToAuctionUrlCommand => new RelayCommand(() => NavigateToAuctionUrl());
        public ICommand NavigateToCountyUrlCommand => RealAuctionNavigateCommand(item => item?.RealAuctionURL);
        public ICommand RealAuctionNavigateCommand(Func<US_County, string> urlSelector) => new RelayCommand<US_County>(item => UrlHelper.OpenUrl(urlSelector(item)), item => item != null && UrlHelper.IsValidUrl(urlSelector(item)));

        #endregion

        #region Properties 
        
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

        public string? AuctionURL
        {
            get => _auctionURL;
            set => SetProperty(ref _auctionURL, value);
        }
        private string? _auctionURL;

        public string? AuctionCounty 
        { 
            get => _auctionCounty; 
            set => SetProperty(ref _auctionCounty, value); 
        }
        private string? _auctionCounty;

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

        #endregion

        #region Constructor

        public RealAuctionViewModel()
        {
            _dataService = new StateDataService();
            SettingsOpenCommand = new RelayCommand(() => ShowCountyEditDialog());
            SettingsCloseCommand = new RelayCommand(() => CloseCountyEditDialog());
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await GetStateList();
            await LoadSettings();
        }

        #endregion

        #region Methods

        private async Task GetStateList()
        {
            States = await _dataService.GetAllStates() as List<US_State>;
            if (States != null && States?.ToList().Count > 0)
                StateSelected = States.ToList()[0];
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

        protected override async Task SaveToClipboard()
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

        #region Real Auction

        private async Task GetRealAuctionHTML()
        {
            if (!CanScrapeRealAuction || IsScraping)
                return;

            ParcelList.Clear();
            var realAuctionHtml = new List<string>();

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
                        var newHtmlTable = await scraper.CaptureHTMLTable();

                        foreach (var htmlTable in newHtmlTable)
                        {
                            // Stop pagination if duplicate found
                            if (realAuctionHtml.Contains(htmlTable))
                            {
                                Console.WriteLine("Item already exists! Stopping pagination.");
                                isLastRecord = true;
                                break;
                            }
                            realAuctionHtml.Add(htmlTable);
                        }

                        // Attempt to click the Next Page button
                        if (!scraper.TryClickNextPage())
                        {
                            Console.WriteLine("Next page button not found. Stopping pagination.");
                            isLastRecord = true;
                        }

                        // Wait briefly between pages to stabilize interactions
                        await Task.Delay(200);
                    }

                    // Filter auction data
                    var filteredList = FilterAuctionData("Auction Starts", "Auction Status", realAuctionHtml);

                    // Assign properties to auction items based on extracted data
                    await GetRealAuctionProperties(filteredList, "Auction Starts");

                    // Process matching parcels for each auction htmlTable
                    //Parallel.ForEach(ParcelList, htmlTable =>
                    //{
                    //    htmlTable.MatchingParcels = GetMatchingParcels(htmlTable.ParcelID, AuctionItems.ToList());
                    //});
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

        private List<string> FilterAuctionData(string lineToKeep, string lineToRemove, List<string> list)
        {
            bool keepData = false;
            var filteredList = new List<string>();

            foreach (var data in list)
            {
                foreach (var line in data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.Contains(lineToKeep))
                        keepData = true;

                    else if (line.Contains(lineToRemove))
                        keepData = false;

                    else if (line.ToLower().Contains("user name"))
                        keepData = false;

                    if (keepData)
                        filteredList.Add(line.Trim());
                }
            }

            return filteredList;
        }

        private async Task GetRealAuctionProperties(List<string> list, string keyword)
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
                                //case string s when span.StartsWith("Case #:"):
                                //    currentItem.DetailUrl = span.Slice("Case #: ".Length).Trim().ToString();
                                //    break;

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
                                        if (AuctionCounty == "Miami Dade")
                                            parcel = currentItem.ParcelID.Replace("-", "");

                                        // Appraiser's office web link
                                        currentItem.AppraiserUrl = string.Format(_preformattedAppraiserUrl, Uri.EscapeDataString(parcel));

                                        // Regrid web link
                                        if (prefix.StartsWith("Parcel ID:"))
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

                    // Add last auction htmlTable
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