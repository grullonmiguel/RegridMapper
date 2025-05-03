using RegridMapper.Core.Commands;
using RegridMapper.Core.Configuration;
using RegridMapper.Models;
using System.Windows.Input;

namespace RegridMapper.ViewModels
{
    public class StateDialogViewModel : BaseDialogViewModel
    {
        #region Commands

        public ICommand CountySelectedCommand => new RelayCommand<string>(GetSelectedCounty);
        public ICommand AppraisalOfficeCommand => new RelayCommand(()=> OpenWebSearch("assessor's office"), CanOpenExternalUrl);
        public ICommand ClerkOfficeCommand => new RelayCommand(()=> OpenWebSearch("clerk's office"), CanOpenExternalUrl);
        public ICommand TaxOfficeCommand => new RelayCommand(() => OpenWebSearch(SalesType == SaleTypeCode.Lien ? "tax lien sale" : "tax deed sale"), CanOpenExternalUrl);

        #endregion

        #region Properties

        public StateCode ID { get; private set; }

        public string? Name { get; private set; }

        public US_County? CountySelected
        {
            get => _countySelected;
            set
            {
                SetProperty(ref _countySelected, value);
                UpdateCountySelected();
            }
        }
        private US_County? _countySelected;

        public List<US_County>? Counties { get; private set; }

        public SaleTypeCode SalesType { get; private set; }

        public string CountyCount { get; private set; }

        #endregion

        #region Constructor

        public StateDialogViewModel(US_State state)
        {
            ID = state.StateID;
            Name = state.Name;
            Counties = state.Counties;
            SalesType = state.SalesType;
            GetSelectedCounty(Counties?.FirstOrDefault()?.Name);

            CountyCount = state?.Counties?.Count <= 1 ?
                $"{state?.Counties?.Count} County" :
                $"{state?.Counties?.Count} Counties";
        }

        #endregion

        #region Methods

        private void UpdateCountySelected()
        {
            if (Counties == null)
                return;

            // Unselect all states first
            Counties.ForEach(c => c.IsSelected = false);

            // Update IsSelected property for selected state
            var selectedCounty = Counties.FirstOrDefault(s => s.Name == CountySelected?.Name);
            if (selectedCounty != null)
                selectedCounty.IsSelected = true;
        }

        private void GetSelectedCounty(string? county)
            => CountySelected = Counties?.FirstOrDefault(x => x.Name == county);

        private bool CanOpenExternalUrl()
            => CountySelected != null;

        private void OpenWebSearch(string keywords)
        {
            if (CountySelected != null)
                UrlHelper.OpenUrl(string.Format(AppConstants.Google_Search, $"{CountySelected?.Name} county, {Name} {keywords}"));
        }

        #endregion

        #region Disposable

        protected override void OnDispose()
        {
            Name = null;
            CountyCount = string.Empty;
            Counties = null;
            CountySelected = null;
            base.OnDispose();
        }

        #endregion
    }
}