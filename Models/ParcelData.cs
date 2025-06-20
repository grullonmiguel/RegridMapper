using RegridMapper.Core.Configuration;
using RegridMapper.Core.Utilities;
using RegridMapper.Models;

namespace RegridMapper.ViewModels
{
    /// <summary>
    /// Represents parcel data extracted from Regrid, fully integrated with MVVM.
    /// </summary>
    public class ParcelData : Observable
    {
        // Property to store multiple matches
        public List<RegridSearchResult> RegridSearchResults
        { 
            get => _regridSearchResults; 
            set => SetProperty(ref _regridSearchResults, value);
        }
        private List<RegridSearchResult> _regridSearchResults = [];

        public string? ParcelID
        {
            get => _parcelID;
            set
            {
                SetProperty(ref _parcelID, value);

                //if (value != null)
                //{
                //    AppraiserUrl = $"https://www.assessormelvinburgess.com/propertyDetails?IR=true&parcelid={ParcelID}";
                //    DetailUrl = $"https://public-sctn.epropertyplus.com/landmgmtpub/remote/public/property/viewSummary?parcelNumber={ParcelID}";
                //}
            }
        }
        private string? _parcelID;

        public decimal? AskingPrice
        {
            get => _askingPrice;
            set => SetProperty(ref _askingPrice, value);
        }
        private decimal? _askingPrice;

        public string? Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }
        private string? _address;

        public string? County
        {
            get => _county;
            set => SetProperty(ref _county, value);
        }
        private string? _county;

        public string? City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }
        private string? _city;

        public string? ZipCode
        {
            get => _zipCode;
            set => SetProperty(ref _zipCode, value);
        }
        private string? _zipCode;

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

        public decimal? AssessedValue
        {
            get => _assessedValue;
            set => SetProperty(ref _assessedValue, value);
        }
        private decimal? _assessedValue;

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

        public string? AppraiserUrl
        {
            get => _appraiserUrl;
            set => SetProperty(ref _appraiserUrl, value);
        }
        private string? _appraiserUrl;

        public string? DetailUrl
        {
            get => _detailUrl;
            set => SetProperty(ref _detailUrl, value);
        }
        private string? _detailUrl;
        // private string? _detailUrl = $"https://public-sctn.epropertyplus.com/landmgmtpub/remote/public/property/viewSummary?parcelNumber={ParcelID}" : "";
        // private string? _detailUrl = !string.IsNullOrWhiteSpace(GeographicCoordinate) ? string.Format(AppConstants.URL_OpenStreetMap, GeographicCoordinate): "";

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
            ? AppConstants.URL_Google.BuildUrl(GeographicCoordinate.ToDMSCoordinates())  : string.Empty;

        /// <summary>
        /// FEMA URL, computed dynamically.
        /// </summary>
        public string FemaUrl => !string.IsNullOrWhiteSpace(GeographicCoordinate)
            ? AppConstants.URL_Fema.BuildUrl(GeographicCoordinate.ToDMSCoordinates()) : string.Empty;

        /// <summary>
        /// Redfin URL, computed dynamically.
        /// </summary>
        public string RedfinUrl => !string.IsNullOrWhiteSpace(ZipCode) && ZipCode.IsValidZipCode()
            ? string.Format(AppConstants.URL_RedfinByZip, ZipCode) : string.Empty;

        /// <summary>
        /// Realtor.com URL, computed dynamically.
        /// </summary>
        public string RealtorUrl => !string.IsNullOrWhiteSpace(ZipCode) && ZipCode.IsValidZipCode()
            ? string.Format(AppConstants.URL_RealtorByZip, ZipCode) : string.Empty;

        /// <summary>
        /// Zillow URL, computed dynamically.
        /// </summary>
        public string ZillowUrl => !string.IsNullOrWhiteSpace(ZipCode) && ZipCode.IsValidZipCode() ? string.Format(AppConstants.URL_ZillowByZip, ZipCode) : string.Empty;

        //public string ZillowUrl => !string.IsNullOrWhiteSpace(ZipCode) && ZipCode.IsValidZipCode()
        //    ? $"https://www.zillow.com/{ZipCode}/sold/?searchQueryState={{\"filterState\":{{\"doz\":{{\"value\":\"90\"}}}}}}"
        //    : string.Empty;

        /// <summary>
        /// Returns a formatted string representing the parcel details.
        /// </summary>
        public override string ToString() => $"ParcelID: {ParcelID}, Address: {Address}";

        public ScrapeStatus ScrapeStatus
        {
            get => _scrapeStatus;
            set => SetProperty(ref _scrapeStatus, value);
        }
        private ScrapeStatus _scrapeStatus;

        public void SetPropertyValue(string propertyName, string value)
        {
            var property = typeof(ParcelData).GetProperty(propertyName);
            if (property != null && !string.IsNullOrEmpty(value))
            {
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                try
                {
                    object convertedValue = Convert.ChangeType(value, targetType);
                    property.SetValue(this, convertedValue);
                }
                catch
                {
                    // Optional: log warning or fallback to a default value
                }
            }
        }

    }
}