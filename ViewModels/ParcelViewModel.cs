using RegridMapper.Core.Commands;
using RegridMapper.Core.Configuration;
using RegridMapper.Core.Utilities;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace RegridMapper.ViewModels
{
    public class ParcelViewModel : BaseViewModel
    {
        #region CanScrape Properties

        public bool ShouldScrapeCity
        {
            get => _shouldScrapeCity;
            set => SetProperty(ref _shouldScrapeCity, value);
        }
        private bool _shouldScrapeCity = true;

        public bool ShouldScrapeZoning
        {
            get => _shouldScrapeZoning;
            set => SetProperty(ref _shouldScrapeZoning, value);
        }
        private bool _shouldScrapeZoning = true;

        public bool ShouldScrapeZipCode
        {
            get => _shouldScrapeZipCode;
            set => SetProperty(ref _shouldScrapeZipCode, value);
        }
        private bool _shouldScrapeZipCode = true;

        public bool ShouldScrapeAddress
        {
            get => _shouldScrapeAddress;
            set => SetProperty(ref _shouldScrapeAddress, value);
        }
        private bool _shouldScrapeAddress = true;

        public bool ShouldScrapeOwner
        {
            get => _shouldScrapeOwner;
            set => SetProperty(ref _shouldScrapeOwner, value);
        }
        private bool _shouldScrapeOwner = true;

        public bool ShouldScrapeAssessedValue
        {
            get => _shouldScrapeAssessedValue;
            set => SetProperty(ref _shouldScrapeAssessedValue, value);
        }
        private bool _shouldScrapeAssessedValue = true;

        public bool ShouldScrapeAcres
        {
            get => _shouldScrapeAcres;
            set => SetProperty(ref _shouldScrapeAcres, value);
        }
        private bool _shouldScrapeAcres = true;

        public bool ShouldScrapeCoordinates
        {
            get => _shouldScrapeCoordinates;
            set => SetProperty(ref _shouldScrapeCoordinates, value);
        }
        private bool _shouldScrapeCoordinates = true;

        public bool ShouldScrapeFloodZone
        {
            get => _shouldScrapeFloodZone;
            set => SetProperty(ref _shouldScrapeFloodZone, value);
        }
        private bool _shouldScrapeFloodZone = true;

        #endregion

        #region Commands

        // Clipboard Commands       
        public ICommand PasteFromClipboardAddressCommand => new RelayCommand(async () => await PasteFromClipboard(true), () => !IsScraping && ParcelList?.Count <= 0);
        public ICommand PasteFromClipboardParcelCommand => new RelayCommand(async () => await PasteFromClipboard(false), () => !IsScraping && ParcelList?.Count <= 0);
       
        // Regrid Scraping Commands
        public ICommand ScrapeRegridSelectedByAddressCommand => new RelayCommand(async () => await ScrapeRegridSelectedParcels(ScrapeType.Address), () => SelectedParcels.Any() && !IsScraping);
        public ICommand ScrapeRegridSelectedByParcelIDCommand => new RelayCommand(async () => await ScrapeRegridSelectedParcels(ScrapeType.Parcel), () => SelectedParcels.Any() && !IsScraping);

        #endregion

        #region Constructor

        public ParcelViewModel()
        {
            SettingsOpenCommand = new RelayCommand(() => ShowSettings = true);
            SettingsCloseCommand = new RelayCommand(CloseSettingsDialog);
        } 

        #endregion

        #region Clipboard

        private async Task PasteFromClipboard(bool byAddress = false)
        {
            try
            {
                string clipboardText = Clipboard.GetText();
                var parcels = clipboardText.Split('\n')
                                                 .Select(p => p.Trim()) // Trim early
                                                 .Where(p => !string.IsNullOrEmpty(p)) // Filter empty entries
                                                 .Select(p => new ParcelData
                                                 {
                                                     ParcelID = byAddress ? null : p,
                                                     Address = byAddress ? p : null
                                                 }) // Conditional mapping
                                                 .ToList(); // Convert to list

                ParcelList.Clear();
                ParcelList.AddRange(parcels); // More efficient than adding one by one

                await _logger!.LogAsync($"Loaded {ParcelList.Count} parcels from clipboard.");
            }
            catch (Exception ex)
            {
                await _logger!.LogExceptionAsync(ex);
            }
            finally
            {
                NotifyPropertiesChanged(nameof(CanScrape), nameof(TotalParcels));
            }
        }

        protected override async Task SaveToClipboard()
        {
            if (ParcelList is null || !ParcelList.Any())
                return; // Exit if no parcels are selected

            var clipboardText = new StringBuilder();

            // Define Headers
            string[] headers = { "TYPE", "CITY", "PARCEL ID / REGRID", "ADDRESS", "OWNER", "APPRAISAL", "ASSESSED VALUE", "ACRES", "FEMA", "ZILLOW", "REDFIN", "REALTOR" };

            // Append headers
            clipboardText.AppendLine(string.Join("\t", headers));

            // Helper method to format hyperlinks correctly for Google Sheets
            static string FormatUrl(string url, string alias) => string.IsNullOrWhiteSpace(url) ? string.Empty : $"=HYPERLINK(\"{url.Replace("\"", "\"\"")}\", \"{alias}\")";

            foreach (var item in ParcelList)
            {
                // Generate hyperlinks with correct spreadsheet formatting
                var urls = new[]
                {
                    FormatUrl(item.RegridUrl, item.ParcelID),
                    FormatUrl(item.GoogleUrl, item.Address),
                    FormatUrl(item.AppraiserUrl, "LINK"),
                    FormatUrl(item.FemaUrl, $"{item.FloodZone}"),
                    FormatUrl(item.ZillowUrl, "Zillow"),
                    FormatUrl(item.RedfinUrl, "Redfin"),
                    FormatUrl(item.RealtorUrl, "Realtor")
                };

                // Create row data while ensuring Excel formatting compatibility
                string[] row =
                {
                    item.ZoningType, item.City, urls[0], urls[1], 
                    item.OwnerName, urls[2], item.AssessedValue.ToString(),
                    item.Acres, urls[3], urls[4], urls[5], urls[6]
                };

                clipboardText.AppendLine(string.Join("\t", row));
            }

            // Clipboard operation runs on UI thread
            await Application.Current.Dispatcher.InvokeAsync(() => Clipboard.SetText(clipboardText.ToString()));
        }

        #endregion

        #region Settings

        private void CloseSettingsDialog()
        {
            ShowSettings = false;
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                //AuctionURL = SettingsService.LoadSetting<string>("AuctionURL");
                //AuctionCounty = SettingsService.LoadSetting<string>("AuctionCounty");
                //_preformattedAppraiserUrl = SettingsService.LoadSetting<string>("AppraiserURL");
                //OnPropertyChanged(nameof(CanScrape));
            }
            catch (Exception)
            {
                // Provide recovery action
            }
        }

        #endregion

        #region Regrid Scraping

        //private async Task ScrapeParcels(List<ParcelData> parcels, CancellationToken cancellationToken)
        //{
        //    if (parcels == null || parcels.Count == 0)
        //        return; // Avoid unnecessary execution if there are no parcels to process

        //    // Indicate process start
        //    IsScraping = true;
        //    NotifyPropertiesChanged(nameof(CanScrape));

        //    // Record the start time
        //    var startTime = DateTime.Now;

        //    try
        //    {
        //        await Task.Run(async () =>
        //        {
        //            // Connect to a chrome session with debugging enabled
        //            using var scraper = new SeleniumWebDriverService(BrowserType.Chrome);
        //            SemaphoreSlim semaphore = new(3);

        //            // Open Main Regrid Page
        //            await scraper.CaptureHTMLSource("https://app.regrid.com/us#");

        //            for (int i = 0; i < parcels.Count; i++)
        //            {
        //                // Check if cancellation is requested and exit early if so
        //                if (cancellationToken.IsCancellationRequested)
        //                {
        //                    Status = "Scraping process canceled.";
        //                    return;
        //                }

        //                var item = parcels[i];
        //                await semaphore.WaitAsync();

        //                try
        //                {
        //                    Status = $"Processing {i + 1} of {parcels.Count}.";
        //                    CurrentItem = item?.ParcelID;

        //                    // Set initial Regrid URL
        //                    item.RegridUrl = string.Format(AppConstants.URL_Regrid, Uri.EscapeDataString(item.ParcelID));

        //                    // Get the HTML for the selected Parcel ID
        //                    var htmlSource = await scraper.CaptureHTMLSource(item?.RegridUrl);

        //                    // Verify something was scraped
        //                    if (string.IsNullOrWhiteSpace(htmlSource))
        //                    {
        //                        item.ScrapeStatus = ScrapeStatus.NotFound;
        //                        CurrentItem = $"NOT FOUND: {item?.ParcelID}";
        //                        await _logger.LogAsync($"Empty response for Parcel ID: {item.ParcelID}");
        //                        continue;
        //                    }

        //                    await _regriDataService.GetParcelData(htmlSource, item, scraper);
        //                }
        //                catch (WebDriverTimeoutException ex)
        //                {
        //                    await _logger.LogExceptionAsync(ex);
        //                }
        //                catch (WebDriverException ex)
        //                {
        //                    await _logger.LogExceptionAsync(ex);
        //                }
        //                catch (Exception ex)
        //                {
        //                    await _logger.LogExceptionAsync(ex);
        //                }
        //                finally
        //                {
        //                    semaphore.Release();
        //                    OnPropertyChanged(nameof(ParcelList));
        //                }
        //            }
        //        }, cancellationToken);

        //    }
        //    finally
        //    {
        //        // Display ellapsed time in minutes and seconds
        //        var elapsedTime = DateTime.Now - startTime;
        //        Status = $"Completed in {elapsedTime.Minutes} minutes and {elapsedTime.Seconds} seconds";

        //        IsScraping = false; // Indicate process end
        //        CurrentItem = string.Empty;
        //        NotifyPropertiesChanged(nameof(IsScraping), nameof(Status), nameof(CanScrape));
        //    }
        //}

        #endregion
    }
}