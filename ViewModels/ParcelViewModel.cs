using OpenQA.Selenium;
using RegridMapper.Core.Commands;
using RegridMapper.Core.Configuration;
using RegridMapper.Core.Services;
using RegridMapper.Core.Utilities;
using RegridMapper.Services;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace RegridMapper.ViewModels
{
    public class ParcelViewModel : BaseViewModel
    {
        #region Fields

        private bool _cancelScraping; 
        private readonly Logger _logger;
        private readonly SeleniumWebDriverService _scraper;
        private readonly RegridDataService _regriDataService = new();
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

        public ICommand ClearDataCommand => new RelayCommand(ClearData);
        public ICommand SelectedParcelsCommand => new RelayCommand<IList>(OnSelectedParcelsChanged);

        // Clipboard Commands
        public ICommand CopySelectedParcelsCommand => new RelayCommand(async () => await SaveToClipboard(), () => ParcelsSelected);
        public ICommand LoadFromClipboardCommand => new RelayCommand(async () => await LoadFromClipboard());

        // Regrid Scraping Commands
        public ICommand RegridQueryCancelCommand => new RelayCommand(async () => await CancelScraping());
        public ICommand RegridQueryAllParcelsCommand => new RelayCommand(async () => await ScrapeParcels(ParcelList.ToList()));
        public ICommand RegridQuerySelectedCommand => new RelayCommand(async () => await ScrapeParcels(SelectedParcels.ToList()), () => ParcelsSelected);

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
        } 

        #endregion

        #region Regrid Scraping

        private async Task ScrapeParcels(List<ParcelData> parcels)
        {
            if (parcels == null || parcels.Count == 0)
                return; // Avoid unnecessary execution if there are no parcels to process
            
            // Indicate process start
            IsScraping = true; 

            // Record the start time
            var startTime = DateTime.Now;

            try
            {
                await Task.Run(async () =>
                {
                    // Connect to a chrome session with debugging enabled
                    using var scraper = new SeleniumWebDriverService(BrowserType.Chrome, true, "127.0.0.1:9222");
                    SemaphoreSlim semaphore = new(3);

                    for (int i = 0; i < parcels.Count; i++)
                    {
                        var item = parcels[i];
                        await semaphore.WaitAsync();

                        try
                        {
                            RegridStatus = $"Processing {i + 1} of {parcels.Count}.";
                            CurrentScrapingElement = item?.ParcelID;

                            // Set initial Regrid URL
                            item.RegridUrl = string.Format(AppConstants.URL_Regrid, Uri.EscapeDataString(item.ParcelID));

                            // Get the HTML for the selected Parcel ID
                            var htmlSource = await scraper.CaptureHTMLSource(item?.RegridUrl);

                            // Verify something was scraped
                            if (string.IsNullOrWhiteSpace(htmlSource))
                            {
                                item.NoMatchDetected = true;
                                CurrentScrapingElement = $"NOT FOUND: {item?.ParcelID}";
                                await _logger.LogAsync($"Empty response for Parcel ID: {item.ParcelID}");
                                continue;
                            }

                            await _regriDataService.GetParcelData(htmlSource, item, scraper);
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
                        }
                    }
                });
            }
            finally
            {
                // Display ellapsed time in minutes and seconds
                var elapsedTime = DateTime.Now - startTime;                
                RegridStatus = $"Completed in {elapsedTime.Minutes} minutes and {elapsedTime.Seconds} seconds";

                IsScraping = false; // Indicate process end
                CurrentScrapingElement = string.Empty;
                NotifyPropertiesChanged(nameof(IsScraping), nameof(RegridStatus));
            }
        }

        private Task CancelScraping()
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

        private async Task SaveToClipboards()

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
                var regridURL   = string.IsNullOrWhiteSpace(item.RegridUrl)   ? "" : string.Format(AppConstants.HYPERLINK_FORMAT, item.RegridUrl, "LINK");
                var mapsURL     = string.IsNullOrWhiteSpace(item.GoogleUrl)   ? "" : string.Format(AppConstants.HYPERLINK_FORMAT, item.GoogleUrl.Replace("\"", "\"\""), "LINK");
                var femaURL     = string.IsNullOrWhiteSpace(item.FemaUrl)     ? "" : string.Format(AppConstants.HYPERLINK_FORMAT, item.FemaUrl.Replace("\"", "\"\""), item.FloodZone);
                var realtorURL  = string.IsNullOrWhiteSpace(item.RealtorUrl)  ? "" : string.Format(AppConstants.HYPERLINK_FORMAT, item.RealtorUrl.Replace("\"", "\"\""), "LINK");
                var redfinURL   = string.IsNullOrWhiteSpace(item.RedfinUrl)   ? "" : string.Format(AppConstants.HYPERLINK_FORMAT, item.RedfinUrl.Replace("\"", "\"\""), "LINK");
                var zillowURL   = string.IsNullOrWhiteSpace(item.ZillowUrl)   ? "" : string.Format(AppConstants.HYPERLINK_FORMAT, item.ZillowUrl.Replace("\"", "\"\""), "LINK");

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

        private async Task SaveToClipboard()
        {
            if (SelectedParcels is null || !SelectedParcels.Any())
                return; // Exit if no parcels are selected

            var clipboardText = new StringBuilder();

            // Define Headers
            string[] headers = { "TYPE", "COUNTY", "CITY", "PARCEL ID", "GIS", "DETAIL", "ADDRESS", "OWNER", "APPRAISAL", "ASSESSED VALUE", "ACRES", "MAPS", "FEMA", "REALTOR", "REDFIN", "ZILLOW" };

            // Append headers
            clipboardText.AppendLine(string.Join("\t", headers));

            // Helper method to format hyperlinks correctly for Google Sheets
            static string FormatUrl(string url, string alias) => string.IsNullOrWhiteSpace(url) ? string.Empty : $"=HYPERLINK(\"{url.Replace("\"", "\"\"")}\", \"{alias}\")";

            foreach (var item in SelectedParcels)
            {
                // Generate hyperlinks with correct spreadsheet formatting
                var urls = new[]
                {
                    FormatUrl(item.RegridUrl, "LINK"),
                    FormatUrl(item.DetailUrl, "LINK"),
                    FormatUrl(item.AppraisalUrl, "LINK"),
                    FormatUrl(item.GoogleUrl, "LINK"),
                    FormatUrl(item.FemaUrl, item.FloodZone),
                    FormatUrl(item.RealtorUrl, "LINK"),
                    FormatUrl(item.RedfinUrl, "LINK"),
                    FormatUrl(item.ZillowUrl, "LINK")
                };

                // Create row data while ensuring Excel formatting compatibility
                string[] row =
                {
                    item.ZoningType, item.County, item.City, item.ParcelID, urls[0],urls[1],
                    item.Address, item.OwnerName, urls[2], item.AssessedValue, item.Acres,
                    urls[3], urls[4], urls[5], urls[6], urls[7]
                };

                clipboardText.AppendLine(string.Join("\t", row));
            }

            // Clipboard operation runs on UI thread
            await Application.Current.Dispatcher.InvokeAsync(() => Clipboard.SetText(clipboardText.ToString()));
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

            NotifyPropertiesChanged(nameof(SelectedParcels),nameof(ParcelsSelected));
        }

        private void NotifyPropertiesChanged(params string[] propertyNames)
        {
            foreach (var property in propertyNames)
                OnPropertyChanged(property);
        }

        private void UpdateRegridStatusLabel(ParcelData item, string message) => CurrentScrapingElement = $"{message} - {item.ParcelID} - ";

        #endregion
    }
}