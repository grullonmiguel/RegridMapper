using RegridMapper.Core.Configuration;

namespace RegridMapper.Core.Services
{
    public static class CountyUrlHelper
    {
        public static string GetUrl(State state, County county, DateTime auctionDate)
        {
            return Urls.TryGetValue((state, county), out var url)
                ? string.Format(url, auctionDate.ToString("MM/dd/yyyy"))
                : string.Empty;
        }

        private static readonly Dictionary<(State, County), string> Urls = new Dictionary<(State, County), string>
        {
            { (State.FL, County.Alachua), "https://alachua.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Baker), "https://baker.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Bay), "https://bay.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Citrus), "https://citrus.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Clay), "https://clay.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Duval), "https://duval.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Escambia), "https://escambia.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Flagler), "https://flagler.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Gilchrist), "https://gilchrist.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Gulf), "https://gulf.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Hendry), "https://hendry.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Hernando), "https://hernando.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Hillsborough), "https://hillsborough.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.IndianRiver), "https://indian-river.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Jackson), "https://jackson.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Lake), "https://lake.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Lee), "https://lee.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Leon), "https://leon.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Marion), "https://marion.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Martin), "https://martin.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.MiamiDade), "https://miami-dade.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Monroe), "https://monroe.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Nassau), "https://nassau.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Okaloosa), "https://okaloosa.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Orange), "https://orange.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Osceola), "https://osceola.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.PalmBeach), "https://palmbeach.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Pasco), "https://pasco.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Pinellas), "https://pinellas.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Polk), "https://polk.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Putnam), "https://putnam.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.SantaRosa), "https://santarosa.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Sarasota), "https://sarasota.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Seminole), "https://seminole.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Volusia), "https://volusia.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" },
            { (State.FL, County.Washington), "https://washington.realtaxdeed.com/index.cfm?zaction=AUCTION&Zmethod=PREVIEW&AUCTIONDATE={0}" }
        };
    }
}