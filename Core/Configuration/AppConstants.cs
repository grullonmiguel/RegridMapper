using System.ComponentModel;
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
    
    public enum State
    {
        [Description("Florida")]
        FL,

        [Description("Texas")]
        TX,

        [Description("Georgia")]
        GA
    }

    public enum ScrapeStatus
    {
        Default,
        [Description("Complete")]
        Complete,
        [Description("Multiple Matches")]
        MultipleMatches,
        [Description("Not Found")]
        NotFound
    }
    
    public enum County
    {
        [Description("Alachua County")]
        Alachua,

        [Description("Baker County")]
        Baker,

        [Description("Bay County")]
        Bay,

        [Description("Citrus County")]
        Citrus,

        [Description("Clay County")]
        Clay,

        [Description("Duval County")]
        Duval,

        [Description("Escambia County")]
        Escambia,

        [Description("Flagler County")]
        Flagler,

        [Description("Gilchrist County")]
        Gilchrist,

        [Description("Gulf County")]
        Gulf,

        [Description("Hendry County")]
        Hendry,

        [Description("Hernando County")]
        Hernando,

        [Description("Hillsborough County")]
        Hillsborough,

        [Description("Indian River County")]
        IndianRiver,

        [Description("Jackson County")]
        Jackson,

        [Description("Lake County")]
        Lake,

        [Description("Lee County")]
        Lee,

        [Description("Leon County")]
        Leon,

        [Description("Marion County")]
        Marion,

        [Description("Martin County")]
        Martin,

        [Description("Miami-Dade County")]
        MiamiDade,

        [Description("Monroe County")]
        Monroe,

        [Description("Nassau County")]
        Nassau,

        [Description("Okaloosa County")]
        Okaloosa,

        [Description("Orange County")]
        Orange,

        [Description("Osceola County")]
        Osceola,

        [Description("Palm Beach County")]
        PalmBeach,

        [Description("Pasco County")]
        Pasco,

        [Description("Pinellas County")]
        Pinellas,

        [Description("Polk County")]
        Polk,

        [Description("Putnam County")]
        Putnam,

        [Description("Santa Rosa County")]
        SantaRosa,

        [Description("Sarasota County")]
        Sarasota,

        [Description("Seminole County")]
        Seminole,

        [Description("Volusia County")]
        Volusia,

        [Description("Washington County")]
        Washington
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

        // Real Auction Constants
        public const string FL_AlachuaCounty = @"https://alachua.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_BakerCounty = @"https://baker.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_BayCounty = @"https://bay.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_CitrusCounty = @"https://citrus.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_ClayCounty = @"https://clay.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_DuvalCounty = @"https://duval.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_EscambiaCounty = @"https://escambia.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_FlaglerCounty = @"https://flagler.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_GilchristCounty = @"https://gilchrist.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_GulfCounty = @"https://gulf.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_HendryCounty = @"https://hendry.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_HernandoCounty = @"https://hernando.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_HillsboroughCounty = @"https://hillsborough.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_IndianRiverCounty = @"https://indian-river.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_JacksonCounty = @"https://jackson.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_LakeCounty = @"https://lake.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_LeeCounty = @"https://lee.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_LeonCounty = @"https://leon.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_MarionCounty = @"https://marion.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_MartinCounty = @"https://martin.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_MiamiDadeCounty = @"https://miami-dade.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_MonroeCounty = @"https://monroe.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_NassauCounty = @"https://nassau.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_OkaloosaCounty = @"https://okaloosa.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_OrangeCounty = @"https://orange.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_OsceolaCounty = @"https://osceola.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_PalmBeachCounty = @"https://palmbeach.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_PascoCounty = @"https://pasco.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_PinellasCounty = @"https://pinellas.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_PolkCounty = @"https://polk.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_PutnamCounty = @"https://putnam.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_SantaRosaCounty = @"https://santarosa.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_SarasotaCounty = @"https://sarasota.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_SeminoleCounty = @"https://seminole.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_VolusiaCounty = @"https://volusia.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        public const string FL_WashingtonCounty = @"https://washington.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";


    }
}