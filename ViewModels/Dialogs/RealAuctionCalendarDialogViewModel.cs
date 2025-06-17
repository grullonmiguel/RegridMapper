using RegridMapper.Core.Commands;
using RegridMapper.Core.Services;
using RegridMapper.Models;
using System.Windows.Input;

namespace RegridMapper.ViewModels
{
    public class RealAuctionCalendarDialogViewModel : BaseDialogViewModel
    {
        #region Fields

        private readonly StateDataService? _dataService;

        #endregion

        #region Commands

        public ICommand SaveCommand => new RelayCommand(SaveSettings);        // URL Navigation Commands
        public ICommand NavigateToCountyUrlCommand => RealAuctionNavigateCommand(item => item?.RealAuctionURL);
        public ICommand RealAuctionNavigateCommand(Func<US_County, string> urlSelector) => new RelayCommand<US_County>(item => UrlHelper.OpenUrl(urlSelector(item)), item => item != null && UrlHelper.IsValidUrl(urlSelector(item)));


        #endregion

        #region Properties

        public DateTime? AuctionDate
        {
            get => _auctionDate;
            set
            {
                if (_auctionDate != value && value != default(DateTime))
                {
                    SetProperty(ref _auctionDate, value);
                    //  UpdateAuctionUrl();
                    //OnPropertyChanged(nameof(CanScrape));
                }
            }
        }
        private DateTime? _auctionDate;

        public IEnumerable<US_State>? States
        {
            get => _states;
            set => SetProperty(ref _states, value);
        }
        private IEnumerable<US_State>? _states;

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

        public List<US_County>? Counties
        {
            get => _counties;
            set => SetProperty(ref _counties, value);
        }
        private List<US_County>? _counties;

        public US_County? CountySelected
        {
            get => _countySelected;
            set
            {
                if (_countySelected != value)
                {
                    SetProperty(ref _countySelected, value);
                    //AuctionCounty = value?.Name ?? string.Empty;
                    //_preformattedAuctionUrl = value?.AuctionUrl ?? string.Empty;
                    //_preformattedAppraiserUrl = value?.AppraiserUrl ?? string.Empty;
                    //UpdateAuctionUrl();
                    //OnPropertyChanged(nameof(CanScrape));
                }
            }
        }
        private US_County? _countySelected;

        #endregion

        #region Constructor

        public RealAuctionCalendarDialogViewModel()
        {
            _dataService = new StateDataService();
            InitializeAsync();
        }

        #endregion

        #region Methods

        private async void InitializeAsync()
        {
            await GetStateList();
        }

        private async Task GetStateList()
        {
            States = await _dataService!.GetAllStates() as List<US_State>;
            //if (States != null && States?.ToList().Count > 0)
            //    StateSelected = States.ToList()[0];
        }

        public void LoadSettings()
        {
            try
            {
                LoadStateSelected();
                LoadCountySelected();
                LoadAuctionDateSelected();
            }
            catch (Exception ex)
            {
                Task.Run(async () => await _logger!.LogExceptionAsync(ex));
            }
        }

        private void LoadStateSelected()
        {
            try
            {
                var stateCode = SettingsService.LoadSetting<string>("AuctionState");
                if (!string.IsNullOrWhiteSpace(stateCode) && States!.Any())
                {
                    StateSelected = States?.FirstOrDefault(s => s.StateID.ToString() == stateCode);
                }
            }
            catch (Exception ex)
            {
                Task.Run(async () => await _logger!.LogExceptionAsync(ex));
            }
        }

        private void LoadCountySelected()
        {
            try
            {
                var countyCode = SettingsService.LoadSetting<string>("AuctionCounty");
                if (!string.IsNullOrWhiteSpace(countyCode) && Counties!.Any())
                {
                    CountySelected = Counties?.FirstOrDefault(c => c.Name == countyCode);
                }
            }
            catch (Exception ex)
            {
                Task.Run(async () => await _logger!.LogExceptionAsync(ex));
            }
        }

        private void LoadAuctionDateSelected()
        {
            try
            {
                var auctionDateString = SettingsService.LoadSetting<string>("AuctionDate");
                if (DateTime.TryParse(auctionDateString, out var auctionDate))
                {
                    AuctionDate = auctionDate;
                }
            }
            catch (Exception ex)
            {
                Task.Run(async () => await _logger!.LogExceptionAsync(ex));
            }
        }

        private void SaveSettings()
        {
            if (StateSelected != null)
                SaveSettings("AuctionState", StateSelected!.StateID!.ToString());

            if (CountySelected != null)
                SaveSettings("AuctionCounty", CountySelected!.Name!.ToString());

            if (AuctionDate.HasValue)
                SaveSettings("AuctionDate", AuctionDate.Value.ToString());

            OkSelected();
        }

        private void SaveSettings(string key, string value)
        {
            try
            {
                SettingsService.SaveSetting(key, value);
            }
            catch (Exception ex)
            {
                Task.Run(async () => await _logger!.LogExceptionAsync(ex));
            }
        }

        #endregion
    }
}
