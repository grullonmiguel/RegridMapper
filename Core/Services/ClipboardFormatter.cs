using RegridMapper.Core.Configuration;
using RegridMapper.ViewModels;

namespace RegridMapper.Core.Services
{
    /// <summary>
    /// A simple formatter to save data to the clipboard
    /// </summary>
    public class ClipboardFormatter
    {
        /// <summary>
        /// Guarantees eachheaderis perfectly synchronized with its data
        /// </summary>
        public readonly List<(string Header, Func<ParcelData, string> GetValue)> _columns;

        /// <summary>
        /// The constructor builds the column list based on the desired output format.
        /// </summary>
        public ClipboardFormatter(ClipboardHeaderType headerType)
        {
            switch (headerType)
            {
                case ClipboardHeaderType.RealAuction:
                    // --- HANDLE REAL AUCTION HEADERS

                    // Insert columns at specific positions or add to the end as needed.
                    _columns = new List<(string Header, Func<ParcelData, string>)>
                    {
                        ("OPENING BID", p => p.AskingPrice?.ToString() ?? ""),
                        ("PARCEL ID", p => p.ParcelID),
                        ("GIS", p => FormatGoogleSheetsUrRL(p.RegridUrl, "Regrid")),
                        ("ADDRESS", p => p.Address),
                        ("APPRAISER", p => FormatGoogleSheetsUrRL(p.AppraiserUrl, "Appraiser")),
                        ("ASSESSED VALUE",  p => p.AssessedValue?.ToString() ?? "")
                    };
                    break;
                case ClipboardHeaderType.Regrid:
                    // --- HANDLE REGRID HEADERS

                    // Insert columns at specific positions or add to the end as needed.
                    _columns = new List<(string Header, Func<ParcelData, string>)>
                    {
                        ("ZONING TYPE", p => p.ZoningType),
                        ("ZONING CODE", p => p.ZoningCode),
                        ("CITY", p => p.City),
                        ("PARCEL ID", p => p.ParcelID),
                        ("GIS", p => FormatGoogleSheetsUrRL(p.RegridUrl, "Regrid")),
                        ("ADDRESS", p => p.Address),
                        ("OWNER NAME", p => p.OwnerName),
                        ("APPRAISER", p => FormatGoogleSheetsUrRL(p.AppraiserUrl, "Appraiser")),
                        ("ASSESSED VALUE",  p => p.AssessedValue?.ToString() ?? ""),
                        ("ACRES", p => p.Acres),
                        ("MAPS", p => FormatGoogleSheetsUrRL(p.GoogleUrl, "Maps")),
                        ("FEMA", p => FormatGoogleSheetsUrRL(p.FemaUrl, $"{p.FloodZone}")),
                        ("ZILLOW", p => FormatGoogleSheetsUrRL(p.ZillowUrl, "Zillow")),
                        ("REDFIN", p => FormatGoogleSheetsUrRL(p.RedfinUrl, "Redfin")),
                        ("REALTOR", p => FormatGoogleSheetsUrRL(p.RealtorUrl, "Realtor")),
                        ("GEOGRAPHIC COORDINATES", p => p.GeographicCoordinate),
                        ("OPENING BID", p => p.AskingPrice?.ToString() ?? "")
                    };
                    break;
            }
        }

        public IEnumerable<string> GetHeaders() => _columns.Select(c => c.Header);

        public IEnumerable<string> FormatRow(ParcelData parcel) => _columns.Select(c => c.GetValue(parcel));

        /// <summary>
        /// Creates a Google Sheets compatible hyperlink
        /// </summary>
        private string FormatGoogleSheetsUrRL(string url, string alias) => string.IsNullOrWhiteSpace(url)
            ? string.Empty : $"=HYPERLINK(\"{url.Replace("\"", "\"\"")}\", \"{alias}\")";
    }
}
