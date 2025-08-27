using RegridMapper.Core.Configuration;
using RegridMapper.Models;

namespace RegridMapper.Core.Services
{
    public static class StateCountyDataFactory
    {
        /// <summary>
        /// List of all available states and their counties.
        /// </summary>
        public static List<US_State> AllStates() => _allStates.Value;

        // Lazy<T> ensures the expensive initialization runs only once, the first time it's accessed.
        private static readonly Lazy<List<US_State>> _allStates = new(() =>
        {
            var states = new List<US_State>
            {
                // --- Florida ---
                new() {StateID = StateCode.FL, Name = "Florida", Counties = GetFloridaCounties()},

                // --- Washington ---
                new() {StateID = StateCode.WA, Name = "Washington", Counties = GetWashingtonStateCounties()}
            };
            return states;
        });

        private static US_County GetCounty(StateCode state, string name, string auctionUrl, string appraisalUrl = "", string detailUrl = "") =>
            new()
            {
                Name = name,
                StateID = state,
                AuctionUrl = auctionUrl,
                AppraiserUrl = appraisalUrl,
                DetailsURL = detailUrl
            };

        #region Counties

        private static List<US_County> GetFloridaCounties()
        {
            // 1. Define the raw county data in a readable array of tuples.
            // Tuple format: (Name, AuctionUrl, AppraiserUrl)
            var countyData = new[]
            {
                ("Alachua",        "https://alachua.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",    "https://qpublic.schneidercorp.com/Application.aspx?AppID=1081&LayerID=26490&PageTypeID=4&PageID=10770&Q=320373606&KeyValue={0}" ),
                ("Baker",          "https://baker.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",      "http://bakerpa.com/propertydetails.php?parcel={0}" ),
                ("Bay",            "https://bay.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",        "https://qpublic.schneidercorp.com/application.aspx?AppID=834&LayerID=15170&PageTypeID=4&PageID=6825&Q=192963963&KeyValue={0}" ),
                ("Brevard",        "https://brevard.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",  "https://www.bcpao.us/PropertySearch/#/parcel/{0}" ),
                ("Calhoun",        "https://calhoun.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",  "https://beacon.schneidercorp.com/Application.aspx?AppID=829&LayerID=15004&PageTypeID=4&PageID=6750&KeyValue={0}" ),
                ("Charlotte",      "https://charlotte.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}","http://www.ccappraiser.com/Show_parcel.asp?acct={0}&gen=T&tax=T&bld=T&oth=T&sal=T&lnd=T&leg=T" ),
                ("Citrus",         "https://citrus.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "http://www.citruspa.org/_Web/datalets/datalet.aspx?mode=profileall&UseSearch=no&pin={0}&jur=19&LMparent=20" ),
                ("Clay",           "https://clay.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",       "https://qpublic.schneidercorp.com/Application.aspx?AppID=830&LayerID=15008&PageTypeID=4&PageID=6756&Q=172585046&KeyValue={0}" ),
                ("Duval",          "https://duval.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",      "https://paopropertysearch.coj.net/Basic/Detail.aspx?RE={0}" ),
                ("Escambia",       "https://escambia.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",   "http://www.escpa.org/cama/Detail_a.aspx?s={0}" ),
                ("Flagler",        "https://flagler.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",    "https://qpublic.schneidercorp.com/Application.aspx?AppID=598&LayerID=9801&PageTypeID=4&PageID=4330&Q=1542956115&KeyValue={0}" ),
                ("Gilchrist",      "https://gilchrist.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",  "https://qpublic.schneidercorp.com/Application.aspx?AppID=820&LayerID=15174&PageTypeID=4&PageID=6883&Q=1071567062&KeyValue={0}" ),
                ("Gulf",           "https://gulf.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",       "https://beacon.schneidercorp.com/Application.aspx?AppID=819&LayerID=15077&PageTypeID=4&PageID=6971&KeyValue={0}" ),
                ("Hendry",         "https://hendry.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "https://beacon.schneidercorp.com/Application.aspx?AppID=1105&LayerID=27399&PageTypeID=4&PageID=11143&Q=636712751&KeyValue={0}" ),
                ("Hernando",       "https://hernando.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",   "https://www.hernandocountygis-fl.us/propertysearch/default.aspx?pin={0}" ),
                ("Hillsborough",   "https://hillsborough.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}","http://gis.hcpafl.org/propertysearch/#/search/basic/folio={0}" ),
                ("Indian River",   "https://indian-river.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}","https://qpublic.schneidercorp.com/Application.aspx?AppID=1109&LayerID=27655&PageTypeID=4&PageID=11279&Q=1438314085&KeyValue={0}" ),
                ("Jackson",        "https://jackson.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",    "https://qpublic.schneidercorp.com/Application.aspx?AppID=851&LayerID=15884&PageTypeID=4&KeyValue={0}" ),
                ("Lake",           "https://lake.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",       "https://lakecopropappr.com/property-details.aspx?ParcelID={0}" ),
                ("Lee",            "https://lee.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",        "http://www.leepa.org/Scripts/PropertyQuery/PropertyQuery.aspx?STRAP={0}" ),
                ("Leon",           "https://leon.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",       "http://www.leonpa.org/pt/Datalets/Datalet.aspx?mode=&UseSearch=no&pin={0}" ),
                ("Manatee",        "https://manatee.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",  "https://www.manateepao.com/parcel/?parid={0}" ),
                ("Marion",         "https://marion.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "https://www.pa.marion.fl.us/PropertySearch.aspx" ),
                ("Martin",         "https://martin.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "https://www.pa.martin.fl.us/app/search/pcn/{0}" ),
                ("Miami Dade",     "https://miami-dade.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}", "https://www.miamidade.gov/Apps/PA/propertysearch/#/?folio={0}" ),
                ("Monroe",         "https://monroe.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "https://qpublic.schneidercorp.com/Application.aspx?AppID=605&LayerID=9946&PageTypeID=3&PageID={0}" ),
                ("Nassau",         "https://nassau.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "https://maps.nassauflpa.com/NassauDetails/ParcelSearchResults.html?PIN={0}" ),
                ("Okaloosa",       "https://okaloosa.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",   "https://qpublic.schneidercorp.com/application.aspx?AppID=855&LayerID=15999&PageTypeID=4&PageID=7114&Q=1619219915&KeyValue={0}" ),
                ("Okeechobee",     "https://okeechobee.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}", "http://www.okeechobeepa.com/gis/?pin={0}" ),
                ("Orange",         "https://orange.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "https://ocpaweb.ocpafl.org/parcelsearch/Parcel%20ID/{0}" ),
                ("Osceola",        "https://osceola.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",    "http://ira.property-appraiser.org/PropertySearch/Default.aspx?PIN={0}" ),
                ("Palm Beach",     "https://palmbeach.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",  "https://pbcpao.gov/Property/Details?parcelId=7{0}" ),
                ("Pasco",          "https://pasco.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",      "http://search.pascopa.com/parcel.aspx?parcel={0}" ),
                ("Pinellas",       "https://pinellas.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",   "https://www.pcpao.gov/property-details?s={0}" ),
                ("Polk",           "https://polk.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",       "http://www.polkpa.org/CamaDisplay.aspx?OutputMode=Display&SearchType=RealEstate&Page=FindByID&ParcelID={0}" ),
                ("Putnam",         "https://putnam.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "https://apps.putnam-fl.com/pa/property/?type=api&parcel={0}" ),
                ("St. Lucie",      "https://stlucie.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",  "https://www.paslc.org/RECard/#/propCard/parcel/1406-323-0010-000-4{0}" ),
                ("Santa Rosa",     "https://santarosa.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",  "http://srcpa.gov/Parcel/Index2?parcel={0}" ),
                ("Sarasota",       "https://sarasota.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",   "http://www.sc-pa.com/propertysearch/parcel/{0}" ),
                ("Seminole",       "https://seminole.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",   "https://parceldetails.scpafl.org/ParcelDetailInfo.aspx?PID={0}" ),
                ("Volusia",        "https://volusia.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",    "http://publicaccess.vcgov.org/volusia/search/CommonSearch.aspx?mode=REALPROP&UseSearch=no&altpin={0}"),
                ("Walton",         "https://walton.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",   "https://qpublic.schneidercorp.com/Application.aspx?AppID=835&LayerID=15172&PageTypeID=4&PageID=6829&KeyValue={0}" ),
                ("Washington",     "https://washington.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}", "https://qpublic.schneidercorp.com/Application.aspx?AppID=896&LayerID=16944&PageTypeID=4&PageID=7615&Q=2073331570&KeyValue={0}" )
            };

            // 2. Project the raw data into US_County objects.
            return countyData
                .Select(data => GetCounty(StateCode.FL, data.Item1, data.Item2, data.Item3))
                .ToList();
        }

        private static List<US_County> GetWashingtonStateCounties()
        {
            // 1. Define the raw county data in a readable array of tuples.
            // Tuple format: (Name, AuctionUrl, AppraiserUrl)
            var countyData = new[]
            {
                ("King", "https://king.wa.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}", "https://blue.kingcounty.com/Assessor/eRealProperty/Dashboard.aspx?ParcelNbr={0}" )
            };

            // 2. Project the raw data into US_County objects.
            return countyData
                .Select(data => GetCounty(StateCode.FL, data.Item1, data.Item2, data.Item3))
                .ToList();
        }

        #endregion
    }
}