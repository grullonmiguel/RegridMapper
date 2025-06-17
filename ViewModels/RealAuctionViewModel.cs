using RegridMapper.Core.Commands;
using RegridMapper.Core.Configuration;
using RegridMapper.Core.Utilities;
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
        private  RealAuctionCalendarDialogViewModel _calendarDialogViewModel;

        #endregion

        #region Commands

        public ICommand RealAuctionCalendarCommand => new RelayCommand(OpenRealAuctionDialog);

        // Real Auction Scraping Commands
        public ICommand ScrapeRealAuctionCommand => new RelayCommand(async ()=> await GetRealAuctionHTML(), ()=> CanScrapeRealAuction);
  
        // URL Navigation Commands
        public ICommand NavigateToAuctionUrlCommand => new RelayCommand(() => NavigateToRealAuctionUrl());

        #endregion

        #region Properties 
        
        public bool CanScrapeRealAuction => !string.IsNullOrWhiteSpace(AuctionURL) && !IsScraping;

        public DateTime? AuctionDate
        {
            get => _auctionDate;
            set
            {
                if (_auctionDate != value)
                    SetProperty(ref _auctionDate, value);
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

        #endregion

        #region Constructor

        public RealAuctionViewModel()
        {
            InitializeRealAuctionCalendarSettings();
        }

        #endregion

        #region Methods

        private void InitializeRealAuctionCalendarSettings()
        {
            try
            {
                _calendarDialogViewModel = new RealAuctionCalendarDialogViewModel();
                _calendarDialogViewModel.RequestClose += RefreshRealAuctionValues;
                RefreshRealAuctionValues(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Task.Run(async () => await _logger!.LogExceptionAsync(ex));
            }
        }

        private void RefreshRealAuctionValues(object? sender, EventArgs e)
        {
            try
            {
                _calendarDialogViewModel.LoadSettings();
                
                AuctionDate = _calendarDialogViewModel?.AuctionDate ?? null;
                AuctionCounty = _calendarDialogViewModel?.CountySelected?.Name ?? string.Empty;
                
                _preformattedAuctionUrl = _calendarDialogViewModel?.CountySelected?.AuctionUrl ?? string.Empty;
                _preformattedAppraiserUrl = _calendarDialogViewModel?.CountySelected?.AppraiserUrl ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(_preformattedAuctionUrl) && AuctionDate is not null)
                    AuctionURL = $"{_preformattedAuctionUrl.Replace("{0}", AuctionDate?.ToString("MM/dd/yyyy"))}";

                OnPropertyChanged(nameof(CanScrape));
            }
            catch (Exception ex)
            {
                Task.Run(async () => await _logger!.LogExceptionAsync(ex));
            }
        }

        private void OpenRealAuctionDialog()
        {
            _calendarDialogViewModel.LoadSettings();
            RaiseDialogOpen(_calendarDialogViewModel);
        }

        private void NavigateToRealAuctionUrl()
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

        #region Real Auction Scraping

        private async Task GetRealAuctionHTML()
        {
            if (!CanScrapeRealAuction || IsScraping)
                return;

            ParcelList.Clear();
            var realAuctionHtml = new List<string>();

            Status = $"Navigating to RealAuction for {AuctionCounty} - {AuctionDate!.Value.ToShortDateString()}";
            await Task.Delay(500);

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

                    var dataItem = 0; 
                    while (!isLastRecord)
                    {
                        dataItem++;
                        var newHtmlTable = await scraper.CaptureHTMLTable();

                        Status = $"Downloading HTML page {dataItem}...";

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

                    Status = $"Filtering HTML data...";

                    // Filter auction data
                    var filteredList = FilterRealAuctionHTML("Auction Starts", "Auction Status", realAuctionHtml);

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
                Status = $"Completed in {elapsedTime.Minutes} minutes and {elapsedTime.Seconds} seconds";

                IsScraping = false; // Indicate process end
                OnPropertyChanged(nameof(TotalParcels));
            }
        }

        private List<string> FilterRealAuctionHTML(string lineToKeep, string lineToRemove, List<string> list)
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
            Status = $"Processing HTML data...";
            await Task.Delay(500);

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
    }
}