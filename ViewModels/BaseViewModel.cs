using OpenQA.Selenium;
using RegridMapper.Core.Commands;
using RegridMapper.Core.Configuration;
using RegridMapper.Core.Services;
using RegridMapper.Core.Utilities;
using RegridMapper.Services;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RegridMapper.ViewModels
{
    public class BaseViewModel : Observable
    {
        #region Fields

        protected Logger? _logger;
        protected ScrapeType _scrapeBy;
        protected readonly SeleniumWebDriverService? _scraper;
        protected CancellationTokenSource? _cancellationTokenSource;
        protected readonly RegridDataService _regriDataService = new();
        protected MultipleMatchesDialogViewModel? _multipleMatchesDialogViewModel;

        public event EventHandler<BaseDialogViewModel> OnDialogOpen;

        #endregion

        #region Properties

        /// <summary>
        /// Handles the Regrid user name
        /// </summary>
        public string? UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }
        private string? _userName;

        /// <summary>
        /// Handles the Regrid password
        /// </summary>
        public string? Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        private string? _password;
        
        /// <summary>
        /// Shows scraping status
        /// </summary>
        public string? Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        private string? _status;

        /// <summary>
        /// The current parcel being scraped
        /// </summary>
        public string? CurrentItem
        {
            get => _currentItem;
            set => SetProperty(ref _currentItem, value);
        }
        private string? _currentItem;

        /// <summary>
        /// Set to true when a scraping job is running
        /// </summary>
        public bool IsScraping
        {
            get => _isScraping;
            set => SetProperty(ref _isScraping, value);
        }
        private bool _isScraping;

        /// <summary>
        /// Display sidebar menu when set to true
        /// </summary>
        public bool ShowSettings
        {
            get => _showSettings;
            set => SetProperty(ref _showSettings, value);
        }
        private bool _showSettings;

        /// <summary>
        /// Holds a list of Parcels
        /// </summary>
        public ObservableCollection<ParcelData> ParcelList { get; } = [];

        /// <summary>
        /// Holds the list of parcels highlighted in the Datagrid
        /// </summary>
        public ObservableCollection<ParcelData> SelectedParcels { get; set; } = new();

        /// <summary>
        /// The total number of parcels in the Datagrid
        /// </summary>       
        public string TotalParcels => ParcelList.Count <= 0 ? "" : $"(Total Records: {ParcelList.Count})";

        /// <summary>
        /// Set to true if any parcels are selected
        /// </summary>
        public bool ParcelsSelected => SelectedParcels.Any();

        /// <summary>
        /// 
        /// </summary>
        public bool CanScrape => ParcelList.Count > 0 && !IsScraping;

        /// <summary>
        /// 
        /// </summary>
        public bool RegridColumnsVisible
        {
            get => _regridColumnsVisible;
            set => SetProperty(ref _regridColumnsVisible, value);
        }
        private bool _regridColumnsVisible = false;

        #endregion

        #region Commands

        public ICommand ClearDataCommand => new RelayCommand(ClearData, () => CanClearData());
        public ICommand? SelectedParcelsCommand => new RelayCommand<IList>(OnSelectedParcelsChanged);
        
        // Clipboard Commands
        public ICommand CopyParcelsCommand => new RelayCommand(async () => await SaveToClipboard(), () => ParcelList.Any());

        // Regrid
        public ICommand RegridMultipleMatchesCommand { get; protected set; }
        public ICommand RegridQueryCancelCommand => new RelayCommand( () => _cancellationTokenSource?.Cancel());
        public ICommand ScrapeRegridByAddressCommand => new RelayCommand(async () => await ScrapeRegridParcels(ScrapeType.Address), () => ParcelList.Any() && !IsScraping);
        public ICommand ScrapeRegridByParcelIDCommand => new RelayCommand(async () => await ScrapeRegridParcels(ScrapeType.Parcel), () => ParcelList.Any() && !IsScraping);

        // Settings
        public virtual ICommand? SettingsOpenCommand { get; set; }
        public virtual ICommand? SettingsCloseCommand { get; set; }

        // URL Datagrid Navigation
        public ICommand NavigateAppraiserCommand => CreateNavigateCommand(item => item?.AppraiserUrl);
        public ICommand NavigateToRegridCommand => CreateNavigateCommand(item => item?.RegridUrl);
        public ICommand NavigateDetailsCommand => CreateNavigateCommand(item => item?.DetailUrl);
        public ICommand NavigateToFemaCommand => CreateNavigateCommand(item => item?.FemaUrl);
        public ICommand NavigateToGoogleMapsCommand => CreateNavigateCommand(item => item?.GoogleUrl);
        public ICommand NavigateToRealtorCommand => CreateNavigateCommand(item => item?.RealtorUrl);
        public ICommand NavigateToRedfinCommand => CreateNavigateCommand(item => item?.RedfinUrl);
        public ICommand NavigateToZillowCommand => CreateNavigateCommand(item => item?.ZillowUrl);
        public ICommand CreateNavigateCommand(Func<ParcelData, string> urlSelector) => new RelayCommand<ParcelData>(item => UrlHelper.OpenUrl(urlSelector(item)), item => item != null && UrlHelper.IsValidUrl(urlSelector(item)));

        #endregion

        #region Constructor

        public BaseViewModel()
        {
            RegridMultipleMatchesCommand = new RelayCommand<ParcelData>(async (item) => await ViewMultipleMatches(item));
        }

        #endregion

        #region Methods

        protected void NotifyPropertiesChanged(params string[] propertyNames)
        {
            foreach (var property in propertyNames)
                OnPropertyChanged(property);
        }

        protected bool CanViewMultipleMatches() =>
            !IsScraping &&
            SelectedParcels?.Count == 1;

        protected void MultipleMatchesScrapeChanged(object? sender, string e)
        {
            // Scrape for selected parcel
            ScrapeParcelWithMultipleMaches(e);
            _multipleMatchesDialogViewModel!.ScrapeChanged -= MultipleMatchesScrapeChanged;
        }

        protected void OnSelectedParcelsChanged(IList? selectedItems)
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

            NotifyPropertiesChanged(nameof(SelectedParcels), nameof(ParcelsSelected));
        }

        protected virtual Task SaveToClipboard()
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Regrid

        protected async Task ScrapeParcelWithMultipleMaches(string url)
        {
            _cancellationTokenSource?.Cancel(); // Cancel any previous operation
            _cancellationTokenSource = new CancellationTokenSource();
            await ScrapeRegrid(SelectedParcels.ToList(), _cancellationTokenSource.Token, url);
        }

        protected async Task ScrapeRegridParcels(ScrapeType scrapeBy)
        {
            if (!CanScrape || IsScraping)
                return;

            _scrapeBy = scrapeBy;
            _cancellationTokenSource?.Cancel(); // Cancel any previous operation
            _cancellationTokenSource = new CancellationTokenSource();
            await ScrapeRegrid(ParcelList.ToList(), _cancellationTokenSource.Token, string.Empty);
        }

        protected async Task ScrapeRegrid(List<ParcelData> parcels, CancellationToken cancellationToken, string url = "")
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
                    // Connect to a chrome session
                    using var scraper = new SeleniumWebDriverService("user_name", "user_password!");

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

                            if (string.IsNullOrWhiteSpace(url))
                                await _regriDataService.GetParcelData(htmlSource, item, scraper);
                            else
                                await _regriDataService.GetParcelDataElements(item, scraper);
                        }
                        catch (WebDriverTimeoutException ex) { await _logger!.LogExceptionAsync(ex); }
                        catch (WebDriverException ex) { await _logger!.LogExceptionAsync(ex); }
                        catch (Exception ex) { await _logger!.LogExceptionAsync(ex); }
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

        #region Private Methods

        private void ClearData()
        {
            ParcelList.Clear();
            Status = string.Empty;
            NotifyPropertiesChanged(nameof(CanScrape), nameof(TotalParcels));
        }

        private bool CanClearData()
            => !IsScraping && ParcelList.Count > 0;

        protected void RaiseDialogOpen(BaseDialogViewModel dialogViewModel)
        {
            OnDialogOpen?.Invoke(this, dialogViewModel);
        }

        protected async Task ViewMultipleMatches(ParcelData item)
        {
            await Task.CompletedTask;

            _multipleMatchesDialogViewModel = new MultipleMatchesDialogViewModel(item);
            _multipleMatchesDialogViewModel.ScrapeChanged -= MultipleMatchesScrapeChanged;
            _multipleMatchesDialogViewModel.ScrapeChanged += MultipleMatchesScrapeChanged;

            RaiseDialogOpen(_multipleMatchesDialogViewModel);
        }

        #endregion
    }
}