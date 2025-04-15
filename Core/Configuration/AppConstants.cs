using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace RegridMapper.Core.Configuration
{
    #region Enums

    public enum BrowserType
    {
        Chrome,
        Firefox,
        Edge
    }

    #endregion
    public static class AppConstants
    {
        public const int DefaultTimeoutSeconds = 10;
        public const string AppName = "RegridMapper";
        public const string RegexIPPort = @"^\d{1,3}(\.\d{1,3}){3}:\d{1,5}$";

        // Matches 5-digit ZIP (33101) or ZIP+4 (33101-1234)
        public const string RegexZipCode = @"^\d{5}(-\d{4})?$";


        // URL Constants
        public const string URL_Fema = "https://msc.fema.gov/portal/search?AddressQuery=";
        public const string URL_Google = "https://www.google.com/maps/search/";
        public const string URL_OpenStreetMap = @"https://www.openstreetmap.org/search?query={0}&zoom=20#map=20";
        public const string URL_Regrid = @"https://app.regrid.com/search?query={0}&context=%2Fus&map_id=";
        public const string URL_RedfinByZip = @"https://www.redfin.com/zipcode/{0}/filter/include=sold-3mo";
        public const string URL_RealtorByZip = @"https://www.realtor.com/realestateandhomes-search/{0}/show-recently-sold";
        //public const string URL_ZillowByZip = @"https://www.zillow.com/{0}/sold/";
        public const string URL_ZillowByZip = @"https://www.zillow.com/{0}/sold/?searchQueryState={{""filterState"":{{""doz"":{{""value"":""90""}}}}}}";
        public const string HYPERLINK_FORMAT = "=HYPERLINK(\"{0}\", \"{1}\")";

        // Regrid Scraping Values

        public const string RegridAcres = "Measurements";
        public const string RegridAddress = "Full Address";
        public const string RegridAssessedValue = "Total Parcel Value";
        public const string RegridCity = "Parcel Address City";
        public const string RegridCoordinates = "Centroid Coordinates";
        public const string RegridFema = "FEMA Flood Zone";
        public const string RegridFema2 = "FEMA NRI Risk Rating";
        public const string RegridOwner = "Owner";
        public const string RegridZip = "5 Digit Parcel Zip Code";
        public const string RegridZip2 = "Zip Code";
        public const string RegridZoningType = "Zoning Type";

    }
}