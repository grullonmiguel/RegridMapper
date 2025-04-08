using System.Globalization;
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

    }
}