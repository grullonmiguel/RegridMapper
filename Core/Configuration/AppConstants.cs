using System.Reflection.Emit;

namespace RegridMapper.Core.Configuration
{
    public static class AppConstants
    {
        public const int DefaultTimeoutSeconds = 10;
        public const string BaseGoogleUrl = "https://www.google.com/maps/search/";
        public const string BaseFemaAUrl = "https://msc.fema.gov/portal/search?AddressQuery=";
        public const string BaseRegridUrlPrefix = "https://app.regrid.com/search?query=";
        public const string BaseRegridUrlPostfix = "&context=%2Fus&map_id=";
        public const string BaseHyperlinkFormulaPrefix = "=HYPERLINK(\\";
    }
}