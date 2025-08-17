using RegridMapper.Core.Commands;
using RegridMapper.Core.Configuration;
using RegridMapper.Core.Services;
using RegridMapper.Core.Utilities;
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
        
        public ICommand PasteFromClipboardAddressCommand => new RelayCommand(async () => await PasteFromClipboard(true), () => !IsScraping && ParcelList?.Count <= 0);
        public ICommand PasteFromClipboardParcelCommand => new RelayCommand(async () => await PasteFromClipboard(false), () => !IsScraping && ParcelList?.Count <= 0);

        #endregion

        #region Constructor

        public ParcelViewModel()
        { } 

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

        /// <summary>
        /// Builds a tab-separated string from the ParcelList and saves it to the system clipboard.
        /// </summary>
        protected override async Task SaveToClipboard()
        {
            // 1. Exit early if there's no data.
            if (ParcelList is null || !ParcelList.Any())
                return;

            // 2. Create the correct formatter based on the current state.
            var formatter = new ClipboardFormatter(ClipboardHeaderType.Regrid);

            // 3. Build the string on a background thread to keep the UI responsive.
            string clipboardText = await Task.Run(() =>
            {
                // Create the first line of the output with the column headers, separated by tabs.
                var header = string.Join("\t", formatter.GetHeaders());

                // Transform each parcel into its clipboard row string.
                var rows = ParcelList.Select(item => string.Join("\t", formatter.FormatRow(item)));

                // Combine the header and all data rows into a single string.
                return header + Environment.NewLine + string.Join(Environment.NewLine, rows);
            });

            // 4. Update the clipboard on the UI thread.
            await Application.Current.Dispatcher.InvokeAsync(() => Clipboard.SetText(clipboardText));
        }

        #endregion
    }
}