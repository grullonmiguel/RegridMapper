using RegridMapper.Core.Commands;
using RegridMapper.Core.Configuration;
using RegridMapper.Models;
using System.Windows.Input;

namespace RegridMapper.ViewModels
{
    public class StateDialogViewModel : BaseDialogViewModel
    {
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

        #region Commands

        public ICommand CountySelectedCommand => new RelayCommand<string>(GetSelectedCounty);
        public ICommand AppraisalOfficeCommand => new RelayCommand<US_County>(OpenAppraisalOfficeURL);
        public ICommand ClerkOfficeCommand => new RelayCommand<US_County>(OpenClerksOfficeURL);
        public ICommand TaxOfficeCommand => new RelayCommand<US_County>(OpenTaxOfficeURL);
        public ICommand BestZipCodesCommand => new RelayCommand<US_County>(OpenBestZipCodesURL);
        public ICommand WorstZipCodesCommand => new RelayCommand<US_County>(OpenWorstZipCodesURL);

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
            foreach (var c in Counties)
                c.IsSelected = false;

            // Update IsSelected property for selected state
            foreach (var s in Counties)
            {
                if (s.Name == CountySelected?.Name)
                {
                    s.IsSelected = true;
                    break;
                }
            }
        }

        private void GetSelectedCounty(string? county)
            => CountySelected = Counties?.FirstOrDefault(x => x.Name == county);

        private void OpenAppraisalOfficeURL(US_County? county)
        {
            //_systemService.OpenWebSearch($"{county?.Name} county, {Name} assessor's office");
        }

        private void OpenClerksOfficeURL(US_County? county)
        {
            //_systemService.OpenWebSearch($"{county?.Name} county, {Name} clerk's office");
        }

        private void OpenTaxOfficeURL(US_County? county)
        {
            //_systemService.OpenWebSearch($"{county?.Name} county, {Name} {(SalesType == SaleTypeCode.Lien ? "tax lien sale" : "tax deed sale")}");
        }

        private void OpenBestZipCodesURL(US_County? county)
        {
            //_systemService.OpenWebSearch($"{county?.Name} county, {Name}  best zip codes");
        }

        private void OpenWorstZipCodesURL(US_County? county)
        {
            //_systemService.OpenWebSearch($"{county?.Name} county, {Name}  worst zip codes");
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
