using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegridMapper.Models
{
    /// <summary>
    /// Represents parcel data extracted from Regrid.
    /// </summary>
    public class ParcelData
    {
        public string ParcelID { get; set; }
        public string Address { get; set; }
        public string Acres { get; set; }
        public string OwnerName { get; set; }
        public string AssessedValue { get; set; }
        public string ZoningType { get; set; }

        /// <summary>
        /// Value for the latitude/longitude coordinate system
        /// Generates the Google Map Url as well as the FEMA Url.
        /// </summary>
        public string GeographicCoordinate  { get; set; }

        /// <summary>
        /// https://www.floodsmart.gov/flood-zones-and-maps
        /// </summary>
        public string FloodZone { get; set; }

        // URL values
        public string FemaUrl { get; set; }
        public string GoogleUrl { get; set; }
        public string RegridUrl { get; set; }

        /// <summary>
        /// Returns a formatted string representing the parcel details.
        /// </summary>
        public override string ToString() => $"ParcelID: {ParcelID}, Address: {Address})";
    }
}
