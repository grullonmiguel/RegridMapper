using RegridMapper.Core.Commands;
using RegridMapper.Core.Configuration;
using RegridMapper.Core.Services;
using RegridMapper.Models;
using System.Windows.Input;

namespace RegridMapper.ViewModels
{
    public class MapViewModel :  BaseViewModel
    {
        #region Fields

        private bool _loaded = false;
        private readonly MapStateDataService _stateDataService = new();

        #endregion

        #region Commands

        public ICommand LoadedCommand => new RelayCommand(async () => await OnLoaded());

        public ICommand StateSelectedCommand => new RelayCommand<StateCode>(GetSelectedState);
        
        public ICommand StateViewCommand => new RelayCommand(()=> ShowSelectedState());

        #endregion

        #region Properties

        public List<US_State> States { get; set; } = [];

        public US_State? StateSelected
        {
            get => _stateSelected;
            set
            {
                SetProperty(ref _stateSelected, value);
                UpdateStateSelected();
                OnPropertyChanged(nameof(CountyCount));
                OnPropertyChanged(nameof(States));
            }
        }
        private US_State? _stateSelected;

        public string CountyCount
            => StateSelected?.Counties == null ? string.Empty :
                StateSelected.Counties.Count == 0 ? string.Empty :
                StateSelected.Counties.Count == 1 ? "1 County" :
                $"{StateSelected.Counties.Count} Counties";

        public bool ShowHybridStates
        {
            get => _showHybrid;
            set
            {
                SetProperty(ref _showHybrid, value);
                UpdateStateStatus(SaleTypeCode.Hybrid, value);
            }
        }
        private bool _showHybrid = true;

        public bool ShowDeedStates
        {
            get => _showDeedStates;
            set
            {
                SetProperty(ref _showDeedStates, value);
                UpdateStateStatus(SaleTypeCode.Deed, value);
            }
        }
        private bool _showDeedStates = true;

        public bool ShowLienStates
        {
            get => _showLienStates;
            set
            {
                SetProperty(ref _showLienStates, value);
                UpdateStateStatus(SaleTypeCode.Lien, value);
            }
        }
        private bool _showLienStates = true;

        public bool ShowRedeemableStates
        {
            get => _showRedeemableStates;
            set
            {
                SetProperty(ref _showRedeemableStates, value);
                UpdateStateStatus(SaleTypeCode.Redeemable, value);
            }
        }
        private bool _showRedeemableStates = true;

        #endregion

        #region Constructor

        public MapViewModel()
        {
        }

        #endregion

        #region Methods

        private async Task OnLoaded()
        {
            if (_loaded)
                return;

            await GetStateList();

            if (States != null)
                StateSelected = States.FirstOrDefault();

            OnPropertyChanged(nameof(States));

            _loaded = true;
        }

        private async Task GetStateList()
            => States = (List<US_State>)await _stateDataService.GetAllStates();

        private void GetSelectedState(StateCode state)
            => StateSelected = States?.FirstOrDefault(x => x.StateID == state);

        private void UpdateStateSelected()
        {
            if (States == null)
                return;

            // Unselect all states first
            foreach (var s in States)
                s.IsSelected = false;

            // Update IsSelected property for selected state
            foreach (var s in States)
            {
                if (s.StateID == StateSelected?.StateID)
                {
                    s.IsSelected = true;
                    break;
                }
            }
        }

        private void UpdateStateStatus(SaleTypeCode type, bool enabled)
        {
            foreach(var s in States)
            {
                if (s.SalesType == type)
                    s.IsEnabled = enabled;
            }
        }

        private void ShowSelectedState()
        {
            if (StateSelected != null)
            {
                RaiseDialogOpen(new StateDialogViewModel(StateSelected));
            }
        }

        #endregion
    }
}