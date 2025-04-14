using Newtonsoft.Json;

public class OpenStreetMapResponse
{
    [JsonProperty("place_id")]
    public long PlaceId { get; set; }

    [JsonProperty("licence")]
    public string Licence { get; set; }

    [JsonProperty("osm_type")]
    public string OsmType { get; set; }

    [JsonProperty("osm_id")]
    public long OsmId { get; set; }

    [JsonProperty("lat")]
    public string Latitude { get; set; }

    [JsonProperty("lon")]
    public string Longitude { get; set; }

    [JsonProperty("class")]
    public string Class { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("place_rank")]
    public int PlaceRank { get; set; }

    [JsonProperty("importance")]
    public double Importance { get; set; }

    [JsonProperty("addresstype")]
    public string AddressType { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("display_name")]
    public string DisplayName { get; set; }

    [JsonProperty("address")]
    public Address Address { get; set; }

    [JsonProperty("boundingbox")]
    public List<string> BoundingBox { get; set; }
}

public class Address
{
    [JsonProperty("house_number")]
    public string HouseNumber { get; set; }

    [JsonProperty("road")]
    public string Road { get; set; }

    [JsonProperty("city")]
    public string City { get; set; }

    [JsonProperty("county")]
    public string County { get; set; }

    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("ISO3166-2-lvl4")]
    public string Iso3166Level4 { get; set; }

    [JsonProperty("postcode")]
    public string Postcode { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }

    [JsonProperty("country_code")]
    public string CountryCode { get; set; }
}