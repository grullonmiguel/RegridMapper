using RegridMapper.Core.Configuration;
using RegridMapper.Core.Utilities;

namespace RegridMapper.ViewModels
{
    /// <summary>
    /// Represents parcel data extracted from Regrid, fully integrated with MVVM.
    /// </summary>
    public class ParcelData : BaseViewModel
    {
        public string? ParcelID
        {
            get => _parcelID;
            set => SetProperty(ref _parcelID, value);
        }
        private string? _parcelID;

        public string? Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }
        private string? _address;

        public string? Acres
        {
            get => _acres;
            set => SetProperty(ref _acres, value);
        }
        private string? _acres;

        public string? OwnerName
        {
            get => _ownerName;
            set => SetProperty(ref _ownerName, value);
        }
        private string? _ownerName;

        public string? AssessedValue
        {
            get => _assessedValue;
            set => SetProperty(ref _assessedValue, value);
        }
        private string? _assessedValue;

        public string? ZoningType
        {
            get => _zoningType;
            set => SetProperty(ref _zoningType, value);
        }
        private string? _zoningType;

        /// <summary>
        /// Latitude/Longitude coordinate system.
        /// Updates URLs dynamically upon change.
        /// </summary>
        public string? GeographicCoordinate
        {
            get => _geographicCoordinate;
            set
            {
                SetProperty(ref _geographicCoordinate, value);
                OnPropertyChanged(nameof(GoogleUrl));
                OnPropertyChanged(nameof(FemaUrl));
            }
        }
        private string? _geographicCoordinate;

        public string? FloodZone
        {
            get => _floodZone;
            set => SetProperty(ref _floodZone, value);
        }
        private string? _floodZone;

        public string? RegridUrl
        {
            get => _regridUrl;
            set => SetProperty(ref _regridUrl, value);
        }
        private string? _regridUrl;

        /// <summary>
        /// Google Maps URL, computed dynamically.
        /// </summary>
        public string GoogleUrl => !string.IsNullOrWhiteSpace(GeographicCoordinate)
            ? AppConstants.BaseGoogleUrl.BuildUrl(GeographicCoordinate)  : string.Empty;

        /// <summary>
        /// FEMA URL, computed dynamically.
        /// </summary>
        public string FemaUrl => !string.IsNullOrWhiteSpace(GeographicCoordinate)
            ? AppConstants.BaseFemaAUrl.BuildUrl(GeographicCoordinate) : string.Empty;

        /// <summary>
        /// Returns a formatted string representing the parcel details.
        /// </summary>
        public override string ToString() => $"ParcelID: {ParcelID}, Address: {Address}";
    }
}