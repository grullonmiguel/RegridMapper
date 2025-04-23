using RegridMapper.Core.Configuration;
using RegridMapper.Models;

namespace RegridMapper.Core.Services
{
    public static class StateCountyDataFactory
    {
        public static List<US_State> AllStates() =>
        [
            Florida (StateCode.FL)
        ];

        private static US_County GetCounty(StateCode state, string name, string auctionUrl, string appraisalUrl = "", string detailUrl = "") =>
            new()
            {
                Name = name,
                StateID = state,
                AuctionUrl = auctionUrl,
                AppraiserUrl = appraisalUrl,
                DetailsURL = detailUrl,
                RealAuctionURL = auctionUrl?.IndexOf(".cfm") is int splitIndex and >= 0
                    ? auctionUrl.Substring(0, splitIndex + 4)
                    : auctionUrl
            };

        private static US_State Florida(StateCode state) => new()
        {
            StateID = StateCode.FL,
            Name = "Florida",
            Counties =
            [
                GetCounty(StateCode.FL, "Alachua",        "https://alachua.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",    "https://qpublic.schneidercorp.com/Application.aspx?AppID=1081&LayerID=26490&PageTypeID=4&PageID=10770&Q=320373606&KeyValue={0}" ),
                GetCounty(StateCode.FL, "Baker",          "https://baker.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",      "http://bakerpa.com/propertydetails.php?parcel={0}" ),
                GetCounty(StateCode.FL, "Bay",            "https://bay.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",        "https://qpublic.schneidercorp.com/application.aspx?AppID=834&LayerID=15170&PageTypeID=4&PageID=6825&Q=192963963&KeyValue={0}" ),
                GetCounty(StateCode.FL, "Brevard",        "https://brevard.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",  "https://www.bcpao.us/PropertySearch/#/parcel/{0}" ),
                GetCounty(StateCode.FL, "Calhoun",        "https://calhoun.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",  "https://beacon.schneidercorp.com/Application.aspx?AppID=829&LayerID=15004&PageTypeID=4&PageID=6750&KeyValue={0}" ),
                GetCounty(StateCode.FL, "Charlotte",      "https://charlotte.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}","http://www.ccappraiser.com/Show_parcel.asp?acct={0}&gen=T&tax=T&bld=T&oth=T&sal=T&lnd=T&leg=T" ),
                GetCounty(StateCode.FL, "Citrus",         "https://citrus.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "http://www.citruspa.org/_Web/datalets/datalet.aspx?mode=profileall&UseSearch=no&pin={0}&jur=19&LMparent=20" ),
                GetCounty(StateCode.FL, "Clay",           "https://clay.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",       "https://qpublic.schneidercorp.com/Application.aspx?AppID=830&LayerID=15008&PageTypeID=4&PageID=6756&Q=172585046&KeyValue={0}" ),
                GetCounty(StateCode.FL, "Duval",          "https://duval.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",      "https://paopropertysearch.coj.net/Basic/Detail.aspx?RE={0}" ),
                GetCounty(StateCode.FL, "Escambia",       "https://escambia.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",   "http://www.escpa.org/cama/Detail_a.aspx?s={0}" ),
                GetCounty(StateCode.FL, "Flagler",        "https://flagler.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",    "https://qpublic.schneidercorp.com/Application.aspx?AppID=598&LayerID=9801&PageTypeID=4&PageID=4330&Q=1542956115&KeyValue={0}" ),
                GetCounty(StateCode.FL, "Gilchrist",      "https://gilchrist.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",  "https://qpublic.schneidercorp.com/Application.aspx?AppID=820&LayerID=15174&PageTypeID=4&PageID=6883&Q=1071567062&KeyValue={0}" ),
                GetCounty(StateCode.FL, "Gulf",           "https://gulf.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",       "https://beacon.schneidercorp.com/Application.aspx?AppID=819&LayerID=15077&PageTypeID=4&PageID=6971&KeyValue={0}" ),
                GetCounty(StateCode.FL, "Hendry",         "https://hendry.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "https://beacon.schneidercorp.com/Application.aspx?AppID=1105&LayerID=27399&PageTypeID=4&PageID=11143&Q=636712751&KeyValue={0}" ),
                GetCounty(StateCode.FL, "Hernando",       "https://hernando.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",   "https://www.hernandocountygis-fl.us/propertysearch/default.aspx?pin={0}" ),
                GetCounty(StateCode.FL, "Hillsborough",   "https://hillsborough.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}","http://gis.hcpafl.org/propertysearch/#/search/basic/folio={0}" ),
                GetCounty(StateCode.FL, "Indian River",   "https://indian-river.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}","https://qpublic.schneidercorp.com/Application.aspx?AppID=1109&LayerID=27655&PageTypeID=4&PageID=11279&Q=1438314085&KeyValue={0}" ),
                GetCounty(StateCode.FL, "Jackson",        "https://jackson.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",    "https://qpublic.schneidercorp.com/Application.aspx?AppID=851&LayerID=15884&PageTypeID=4&KeyValue={0}" ),
                GetCounty(StateCode.FL, "Lake",           "https://lake.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",       "https://lakecopropappr.com/property-details.aspx?ParcelID={0}" ),
                GetCounty(StateCode.FL, "Lee",            "https://lee.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",        "http://www.leepa.org/Scripts/PropertyQuery/PropertyQuery.aspx?STRAP={0}" ),
                GetCounty(StateCode.FL, "Leon",           "https://leon.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",       "http://www.leonpa.org/pt/Datalets/Datalet.aspx?mode=&UseSearch=no&pin={0}" ),
                GetCounty(StateCode.FL, "Manatee",        "https://manatee.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",  "https://www.manateepao.com/parcel/?parid={0}" ),
                GetCounty(StateCode.FL, "Marion",         "https://marion.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "https://www.pa.marion.fl.us/PropertySearch.aspx" ),
                GetCounty(StateCode.FL, "Martin",         "https://martin.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "https://www.pa.martin.fl.us/app/search/pcn/{0}" ),
                GetCounty(StateCode.FL, "Miami Dade",     "https://miami-dade.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}", "https://www.miamidade.gov/Apps/PA/propertysearch/#/?folio={0}" ),
                GetCounty(StateCode.FL, "Monroe",         "https://monroe.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "https://qpublic.schneidercorp.com/Application.aspx?AppID=605&LayerID=9946&PageTypeID=3&PageID={0}" ),
                GetCounty(StateCode.FL, "Nassau",         "https://nassau.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "https://maps.nassauflpa.com/NassauDetails/ParcelSearchResults.html?PIN={0}" ),
                GetCounty(StateCode.FL, "Okaloosa",       "https://okaloosa.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",   "https://qpublic.schneidercorp.com/application.aspx?AppID=855&LayerID=15999&PageTypeID=4&PageID=7114&Q=1619219915&KeyValue={0}" ),
                GetCounty(StateCode.FL, "Okeechobee",     "https://okeechobee.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}", "http://www.okeechobeepa.com/gis/?pin={0}" ),
                GetCounty(StateCode.FL, "Orange",         "https://orange.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "https://ocpaweb.ocpafl.org/parcelsearch/Parcel%20ID/{0}" ),
                GetCounty(StateCode.FL, "Osceola",        "https://osceola.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",    "http://ira.property-appraiser.org/PropertySearch/Default.aspx?PIN={0}" ),
                GetCounty(StateCode.FL, "Palm Beach",     "https://palmbeach.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",  "https://pbcpao.gov/Property/Details?parcelId=7{0}" ),
                GetCounty(StateCode.FL, "Pasco",          "https://pasco.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",      "http://search.pascopa.com/parcel.aspx?parcel={0}" ),
                GetCounty(StateCode.FL, "Pinellas",       "https://pinellas.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",   "https://www.pcpao.gov/property-details?s={0}" ),
                GetCounty(StateCode.FL, "Polk",           "https://polk.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",       "http://www.polkpa.org/CamaDisplay.aspx?OutputMode=Display&SearchType=RealEstate&Page=FindByID&ParcelID={0}" ),
                GetCounty(StateCode.FL, "Putnam",         "https://putnam.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",     "https://apps.putnam-fl.com/pa/property/?type=api&parcel={0}" ),
                GetCounty(StateCode.FL, "St. Lucie",      "https://stlucie.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",  "https://www.paslc.org/RECard/#/propCard/parcel/1406-323-0010-000-4{0}" ),
                GetCounty(StateCode.FL, "Santa Rosa",     "https://santarosa.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",  "http://srcpa.gov/Parcel/Index2?parcel={0}" ),
                GetCounty(StateCode.FL, "Sarasota",       "https://sarasota.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",   "http://www.sc-pa.com/propertysearch/parcel/{0}" ),
                GetCounty(StateCode.FL, "Seminole",       "https://seminole.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",   "https://parceldetails.scpafl.org/ParcelDetailInfo.aspx?PID={0}" ),
                GetCounty(StateCode.FL, "Volusia",        "https://volusia.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",    "http://publicaccess.vcgov.org/volusia/search/CommonSearch.aspx?mode=REALPROP&UseSearch=no&altpin={0}"),
                GetCounty(StateCode.FL, "Walton",         "https://walton.realforeclose.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}",   "https://qpublic.schneidercorp.com/Application.aspx?AppID=835&LayerID=15172&PageTypeID=4&PageID=6829&KeyValue={0}" ),
                GetCounty(StateCode.FL, "Washington",     "https://washington.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}", "https://qpublic.schneidercorp.com/Application.aspx?AppID=896&LayerID=16944&PageTypeID=4&PageID=7615&Q=2073331570&KeyValue={0}" )
            ]
        };
    }
}