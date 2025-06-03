using RegridMapper.Core.Commands;
using RegridMapper.Models;
using System.Diagnostics.Metrics;
using System.Windows.Input;
using System.Xml.Linq;

namespace RegridMapper.ViewModels
{
    public class MultipleMatchesDialogViewModel : BaseDialogViewModel
    {
        #region Fields

        private readonly string _regridUrl;
        public event EventHandler<string>? ScrapeChanged;

        #endregion

        #region Commands

        public ICommand ScrapeCommand => new RelayCommand(async () => await ScrapeSelectedRecord(), () => CanScrape());

        #endregion

        #region Properties

        public bool ParcelsSelected => RegridSearchResults.Any();

        public string? ParcelID
        {
            get => _parcelID;
            set => SetProperty(ref _parcelID, value);
        }
        private string? _parcelID;

        public List<RegridSearchResult> RegridSearchResults
        {
            get => _regridSearchResults;
            set => SetProperty(ref _regridSearchResults, value);
        }
        private List<RegridSearchResult> _regridSearchResults = [];
        
        public RegridSearchResult SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }
        private RegridSearchResult _selectedItem;

        #endregion

        #region Constructor

        public MultipleMatchesDialogViewModel(ParcelData parcelData)
        {
            _regridUrl = parcelData.RegridUrl;
            ParcelID = parcelData.ParcelID;
            RegridSearchResults = parcelData.RegridSearchResults;
        }

        #endregion

        #region Methods

        private bool CanScrape()
            => SelectedItem != null;

        private async Task ScrapeSelectedRecord()
        {
            await Task.CompletedTask;
            ScrapeChanged?.Invoke(this, SelectedItem?.ParcelURL);
            OkSelected();
        }

        #endregion

        #region Disposable

        protected override void OnDispose()
        {
            SelectedItem = null;
            RegridSearchResults = null;
            ParcelID = string.Empty;
            base.OnDispose();
        }

        #endregion
    }
}