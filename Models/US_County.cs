using RegridMapper.Core.Configuration;

namespace RegridMapper.Models
{
    public class US_County
    {
        public string? Name { get; set; }
        public StateCode StateID { get; set; }
        public string? AuctionUrl { get; set; }
        public string? AppraiserUrl { get; set; }
        public string? DetailsURL { get; set; }
        public string? RealAuctionURL { get; set; }
    }
}
