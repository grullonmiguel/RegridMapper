using RegridMapper.Core.Configuration;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RegridMapper.Core.Utilities
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string to title case (first letter of each word capitalized).
        /// </summary>
        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(input.ToLower());
        }

        /// <summary>
        /// Returns a substring safely without throwing exceptions.
        /// </summary>
        public static string SafeSubstring(this string input, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(input) || startIndex >= input.Length)
                return string.Empty;

            length = Math.Min(length, input.Length - startIndex);
            return input.Substring(startIndex, length);
        }

        /// <summary>
        /// Checks if a string contains another string, ignoring case.
        /// </summary>
        public static bool ContainsIgnoreCase(this string input, string value)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value))
                return false;

            return input.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Removes all whitespace from a string.
        /// </summary>
        public static string RemoveWhitespace(this string input) 
            => string.IsNullOrWhiteSpace(input) ? string.Empty : string.Join("", input.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

        /// <summary>
        /// Ensures a string is not null or empty, returning a default value if needed.
        /// </summary>
        public static string DefaultIfEmpty(this string input, string defaultValue) 
            => string.IsNullOrWhiteSpace(input) ? defaultValue : input;

        /// <summary>
        /// Appends a geographic coordinate to a base URL.
        /// </summary>
        public static string BuildUrl(this string baseUrl, string urlParameter)
        {
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(urlParameter))
                return string.Empty;

            return $"{baseUrl}{urlParameter}";
        }

        /// <summary>
        /// Validates if the string matches the given regex pattern.
        /// </summary>
        public static bool IsValidFormat(this string input, string regexPattern)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(regexPattern))
                return false;

            return Regex.IsMatch(input, regexPattern);
        }

        /// <summary>
        /// Converts a latitude-longitude string to DMS format.
        /// </summary>
        /// <param name="coordinates">The input string containing latitude and longitude.</param>
        /// <returns>DMS format as a single line.</returns>
        public static string ToDMSCoordinates(this string coordinates)
        {
            if (string.IsNullOrWhiteSpace(coordinates))
                return string.Empty;

            var parts = coordinates.Split(',');
            if (parts.Length != 2 || !double.TryParse(parts[0].Trim(), out double lat) || !double.TryParse(parts[1].Trim(), out double lon))
                return string.Empty;

            return $"{ConvertToDMS(lat, true)} {ConvertToDMS(lon, false)}";
        }

        /// <summary>
        /// Converts a decimal degree coordinate to DMS format.
        /// </summary>
        /// <param name="coordinate">The coordinate value.</param>
        /// <param name="isLatitude">True if latitude, false if longitude.</param>
        /// <returns>The formatted DMS string.</returns>
        private static string ConvertToDMS(double coordinate, bool isLatitude)
        {
            int degrees = (int)coordinate;
            double minutesDecimal = (Math.Abs(coordinate) - Math.Abs(degrees)) * 60;
            int minutes = (int)minutesDecimal;
            double seconds = (minutesDecimal - minutes) * 60;

            char direction = isLatitude
                ? (coordinate >= 0 ? 'N' : 'S')
                : (coordinate >= 0 ? 'E' : 'W');

            return $"{Math.Abs(degrees)}° {minutes}' {seconds:F2}\" {direction}";
        }

        public static bool IsValidZipCode(this string zipCode)
        {
            if (string.IsNullOrWhiteSpace(zipCode))
                return false;

            return Regex.IsMatch(zipCode, AppConstants.RegexZipCode);
        }

        /// <summary>
        /// Extension Method to Retrieve Enum Labels
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute)field.GetCustomAttribute(typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }

        /// <summary>
        /// Converts a string to Pascal Case (capitalizing 
        /// the first letter of each word and removing spaces or separators
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToPascalCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Replace underscores, hyphens, and multiple spaces with a single space
            string cleaned = Regex.Replace(input.ToLower(), @"[_\-]+", " ");
            cleaned = Regex.Replace(cleaned, @"\s+", " ");

            // Convert to title case and remove spaces
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(cleaned).Replace(" ", string.Empty);
        }

        public static string ToPascalCaseWithSpaces(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Normalize spacing and lowercase everything
            string cleaned = Regex.Replace(input.ToLower(), @"\s+", " ").Trim();

            // Convert to title case (Pascal Case with spaces)
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(cleaned);
        }

        public static string ToTitleCaseWithoutNumbers(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove all numbers and leading/trailing spaces
            var stringWithoutNumbers = Regex.Replace(input, @"\d+", "").Trim();

            if (string.IsNullOrWhiteSpace(stringWithoutNumbers))
                return string.Empty;

            // Convert the string to title case
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(stringWithoutNumbers.ToLower());
        }
    }
}