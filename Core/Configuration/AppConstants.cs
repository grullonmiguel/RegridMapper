using System.ComponentModel;

namespace RegridMapper.Core.Configuration
{
    #region Enums

    public enum BrowserType
    {
        Chrome,
        Firefox,
        Edge
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

    public enum SaleTypeCode
    {
        Unknown = 0,
        [Description("Tax Deed")] Deed = 1,
        [Description("Hybrid")] Hybrid = 2,
        [Description("Tax Lien")] Lien = 3,
        [Description("Redeemable Deed")] Redeemable = 4
    }

    /// <summary>
    /// States of the US - For Maps
    /// </summary>
    public enum StateCode
    {
        [Description("Alabama")] AL = 1,
        [Description("Alaska")] AK = 2,
        [Description("Arkansas")] AR = 3,
        [Description("Arizona")] AZ = 4,
        [Description("California")] CA = 5,
        [Description("Colorado")] CO = 6,
        [Description("Connecticut")] CT = 7,
        [Description("District of Columbia")] DC = 8,
        [Description("Delaware")] DE = 9,
        [Description("Florida")] FL = 10,
        [Description("Georgia")] GA = 11,
        [Description("Hawaii")] HI = 12,
        [Description("Iowa")] IA = 13,
        [Description("Idaho")] ID = 14,
        [Description("Illinois")] IL = 15,
        [Description("Indiana")] IN = 16,
        [Description("Kansas")] KS = 17,
        [Description("Kentucky")] KY = 18,
        [Description("Louisiana")] LA = 19,
        [Description("Massachusetts")] MA = 20,
        [Description("Maryland")] MD = 21,
        [Description("Maine")] ME = 22,
        [Description("Michigan")] MI = 23,
        [Description("Minnesota")] MN = 24,
        [Description("Missouri")] MO = 25,
        [Description("Mississippi")] MS = 26,
        [Description("Montana")] MT = 27,
        [Description("North Carolina")] NC = 28,
        [Description("North Dakota")] ND = 29,
        [Description("Nebraska")] NE = 30,
        [Description("New Hampshire")] NH = 31,
        [Description("New Jersey")] NJ = 32,
        [Description("New Mexico")] NM = 33,
        [Description("Nevada")] NV = 34,
        [Description("New York")] NY = 35,
        [Description("Oklahoma")] OK = 36,
        [Description("Ohio")] OH = 37,
        [Description("Oregon")] OR = 38,
        [Description("Pennsylvania")] PA = 39,
        [Description("Rhode Island")] RI = 40,
        [Description("South Carolina")] SC = 41,
        [Description("South Dakota")] SD = 42,
        [Description("Tennessee")] TN = 43,
        [Description("Texas")] TX = 44,
        [Description("Utah")] UT = 45,
        [Description("Virginia")] VA = 46,
        [Description("Vermont")] VT = 47,
        [Description("Washington")] WA = 48,
        [Description("Wisconsin")] WI = 49,
        [Description("West Virginia")] WV = 50,
        [Description("Wyoming")] WY = 51
    }

    #endregion

    public static class AppConstants
    {
        public const int DefaultTimeoutSeconds = 10;
        public const string AppName = "RegridMapper";
        public const string ChromeDebuggerAddress = "127.0.0.1:9222";
        public const string RegexIPPort = @"^\d{1,3}(\.\d{1,3}){3}:\d{1,5}$";        

        // Matches 5-digit ZIP (33101) or ZIP+4 (33101-1234)
        public const string RegexZipCode = @"^\d{5}(-\d{4})?$";

        // URL Constants
        public const string Google_Search = @"http://www.google.com.au/search?q={0}";
        public const string HYPERLINK_FORMAT = "=HYPERLINK(\"{0}\", \"{1}\")";
        public const string URL_Fema = "https://msc.fema.gov/portal/search?AddressQuery=";
        public const string URL_Google = "https://www.google.com/maps/search/";
        public const string URL_OpenStreetMap = @"https://www.openstreetmap.org/search?query={0}&zoom=20#map=20";
        public const string URL_Regrid = @"https://app.regrid.com/search?query={0}&context=%2Fus&map_id=";
        public const string URL_RedfinByZip = @"https://www.redfin.com/zipcode/{0}/filter/include=sold-3mo";
        public const string URL_RealtorByZip = @"https://www.realtor.com/realestateandhomes-search/{0}/show-recently-sold";
        public const string URL_ZillowByZip = @"https://www.zillow.com/{0}/sold/?searchQueryState={{""filterState"":{{""doz"":{{""value"":""90""}}}}}}";

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
        //public const string FL_AlachuaCounty = @"https://alachua.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_BakerCounty = @"https://baker.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_BayCounty = @"https://bay.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_CitrusCounty = @"https://citrus.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_ClayCounty = @"https://clay.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_DuvalCounty = @"https://duval.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_EscambiaCounty = @"https://escambia.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_FlaglerCounty = @"https://flagler.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_GilchristCounty = @"https://gilchrist.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_GulfCounty = @"https://gulf.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_HendryCounty = @"https://hendry.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_HernandoCounty = @"https://hernando.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_HillsboroughCounty = @"https://hillsborough.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_IndianRiverCounty = @"https://indian-river.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_JacksonCounty = @"https://jackson.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_LakeCounty = @"https://lake.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_LeeCounty = @"https://lee.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_LeonCounty = @"https://leon.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_MarionCounty = @"https://marion.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_MartinCounty = @"https://martin.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_MiamiDadeCounty = @"https://miami-dade.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_MonroeCounty = @"https://monroe.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_NassauCounty = @"https://nassau.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_OkaloosaCounty = @"https://okaloosa.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_OrangeCounty = @"https://orange.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_OsceolaCounty = @"https://osceola.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_PalmBeachCounty = @"https://palmbeach.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_PascoCounty = @"https://pasco.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_PinellasCounty = @"https://pinellas.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_PolkCounty = @"https://polk.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_PutnamCounty = @"https://putnam.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_SantaRosaCounty = @"https://santarosa.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_SarasotaCounty = @"https://sarasota.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_SeminoleCounty = @"https://seminole.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_VolusiaCounty = @"https://volusia.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";
        //public const string FL_WashingtonCounty = @"https://washington.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}";


    }
}