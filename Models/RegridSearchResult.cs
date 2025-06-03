namespace RegridMapper.Models
{
    public class RegridSearchResult
    {
        public string? ParcelAddress
        {
            get => _parcelAddress;
            set => _parcelAddress = value;
        }
        private string? _parcelAddress;

        public string? ParcelCity
        {
            get => _parcelCity;
            set => _parcelCity = value;
        }
        private string? _parcelCity;

        public string? ParcelURL
        {
            get => _parcelURL;
            set => _parcelURL = value;
        }
        private string? _parcelURL;
    }
}
