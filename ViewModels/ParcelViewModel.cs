using RegridMapper.Core.Commands;
using RegridMapper.Core.Utilities;
using RegridMapper.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace RegridMapper.ViewModels
{
    public class ParcelViewModel : BaseViewModel
    {
        #region Fields

        private bool _cancelScraping; 
        private readonly Logger _logger;
        private readonly SeleniumScraper _scraper;

        #endregion

        #region Properties

        public ObservableCollection<ParcelData> ParcelList { get; } = [];

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

        public string TotalParcels => ParcelList.Count <= 0 ? "" : $"Total Rows: {ParcelList.Count}";

        #endregion

        #region Commands

        public ICommand LoadFromClipboardCommand { get; }

        public ICommand ScrapeParcelsCommand { get; }

        public ICommand ClearDataCommand { get; }

        public ICommand CancelScrapingCommand { get; }

        #endregion

        public ParcelViewModel()
        {
            _logger = Logger.Instance;
            ClearDataCommand = new RelayCommand(_ => ClearData());
            LoadFromClipboardCommand = new RelayCommand(async _ => await LoadFromClipboard());
            ScrapeParcelsCommand = new RelayCommand(async _ => await ScrapeParcels());
            CancelScrapingCommand = new RelayCommand(async _ => await CancelScraping());
        }

        private void ClearData()
        {
            ParcelList.Clear();
            RegridStatus = string.Empty;
            NotifyPropertiesChanged(nameof(CanPaste), nameof(CanScrape), nameof(TotalParcels));
        }

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

        private async Task ScrapeParcels()
        {
            var index = 1;
            IsScraping = true;

            foreach (var parcel in ParcelList)
            {
                try
                {
                    RegridStatus = $"Processing {index} of {ParcelList.Count} parcels";

                    await Task.Delay(1000);

                    if(_cancelScraping)
                    {
                        IsScraping=false;
                        _cancelScraping = false;
                        RegridStatus = $"Processs Canceled. Completed {index} out of {ParcelList.Count}";
                        return;
                    }

                    //var scrapedData = _scraper.ScrapeParcelData(parcel.ParcelID);
                    //parcel.OwnerName = scrapedData.OwnerName;
                    index += 1;
                }
                catch (Exception ex)
                {
                    _logger.LogExceptionAsync(ex);
                }
            }

            IsScraping = false;
            RegridStatus = $"Completed!";

            await _logger.LogAsync("Parcel scraping completed.");
        }

        private Task CancelScraping()
        {
            _cancelScraping = true;
            return Task.CompletedTask;
        }

        private void NotifyPropertiesChanged(params string[] propertyNames)
        {
            foreach (var property in propertyNames)
                OnPropertyChanged(property);
        }
    }
}