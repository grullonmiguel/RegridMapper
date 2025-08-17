using System.Diagnostics;

public static class UrlHelper
{
    public static string ErrorMessage { get; private set; } = string.Empty;

    /// <summary>
    /// Opens a URL in the default browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static bool OpenUrl(string url)
    {
        bool result = false;
        ErrorMessage = string.Empty;

        try
        {
            if (!IsValidUrl(url))
                throw new Exception($"Invalid URL: {url}. Skipping...");

            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            result = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error opening URL: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Validates if a given string is a proper URL.
    /// Ensures the scheme is HTTP or HTTPS.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}