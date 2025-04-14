using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RegridMapper.Core.Services
{
    public class OpenStreetMapService
    {
        private const string BaseUrl = "https://nominatim.openstreetmap.org/reverse";
        private static readonly HttpClient _httpClient = new();

        public OpenStreetMapService()
        {
            // Set required headers for OpenStreetMap API
            //_httpClient.DefaultRequestHeaders.Add("User-Agent", "YourAppName/1.0 (your.email@example.com)");
            //_httpClient.DefaultRequestHeaders.Add("Referer", "https://yourwebsite.com");
        }

        /// <summary>
        /// Fetches location data from OpenStreetMap using latitude and longitude.
        /// </summary>
        public async Task<OpenStreetMapResponse?> GetLocationDataAsync(double latitude, double longitude)
        {
            try
            {
                string queryUrl = $"{BaseUrl}?lat={latitude}&lon={longitude}&format=json";
                var response = await _httpClient.GetStringAsync(queryUrl);
                return JsonConvert.DeserializeObject<OpenStreetMapResponse>(response);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Fetches location data from OpenStreetMap using a coordinate string formatted as "latitude, longitude".
        /// </summary>
        public async Task<OpenStreetMapResponse?> GetLocationDataAsync(string coordinate)
        {
            try
            {
                var (latitude, longitude) = ParseCoordinateString(coordinate);
                return await GetLocationDataAsync(latitude, longitude);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Invalid coordinate format: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parses a coordinate string formatted as "latitude, longitude" into decimal values.
        /// </summary>
        private static (double Latitude, double Longitude) ParseCoordinateString(string coordinate)
        {
            var parts = coordinate.Split(',');

            if (parts.Length != 2)
                throw new ArgumentException("Invalid format. Expected 'latitude, longitude'.");

            if (!double.TryParse(parts[0].Trim(), out double latitude) || !double.TryParse(parts[1].Trim(), out double longitude))
                throw new ArgumentException("Failed to parse latitude or longitude.");

            return (latitude, longitude);
        }
    }
}