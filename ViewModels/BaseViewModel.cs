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

        #endregion

        #region Commands

        public ICommand ClearDataCommand => new RelayCommand(ClearData, () => CanClearData());
        public ICommand? SelectedParcelsCommand => new RelayCommand<IList>(OnSelectedParcelsChanged);
        public ICommand RegridMultipleMatchesCommand { get; protected set; }
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

        //protected bool CanViewMultipleMatches() =>
        //    !IsScraping &&
        //    SelectedParcels?.Count == 1 &&
        //    SelectedParcels[0]?.ScrapeStatus == ScrapeStatus.MultipleMatches;

        protected bool CanViewMultipleMatches() =>
            !IsScraping &&
            SelectedParcels?.Count == 1;


        protected void MultipleMatchesScrapeChanged(object? sender, string e)
        {
            // Scrape for selected parcel
            ScrapeParcelWithMultipleMaches(e);
            _multipleMatchesDialogViewModel!.ScrapeChanged -= MultipleMatchesScrapeChanged;
        }

        protected virtual async Task ScrapeParcelWithMultipleMaches(string url)
        {
            await Task.CompletedTask; // Placeholder to ensure it compiles correctly
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