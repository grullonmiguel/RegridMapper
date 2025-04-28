using RegridMapper.Core.Configuration;
using System.Diagnostics;
using System.Net.Http;

namespace RegridMapper.Core.Services
{
    public static class GoogleChromeHelper
    {
        /// <summary>
        /// Check if Chrome is Running with Debugging
        /// </summary>
        public static async Task<bool> IsChromeRunning()
        {
            string debuggerAddress = $"http://{AppConstants.ChromeDebuggerAddress}/json";

            using (HttpClient client = new())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(debuggerAddress);
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Chrome is running with debugging enabled on 127.0.0.1:9222.");
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Debugging Info: " + jsonResponse);
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Chrome is running, but debugging is not enabled on 127.0.0.1:9222.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking debugger address: {ex.Message}");
                    return false;
                }
            }
        }

        public static bool LaunchChrome()
        {
            string chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"; // Adjust if necessary
            string arguments = "--remote-debugging-port=9222 --user-data-dir=\"C:\\ChromeSession\"";

            ProcessStartInfo psi = new()
            {
                FileName = chromePath,
                Arguments = arguments,
                UseShellExecute = false
            };

            try
            {
                Process.Start(psi);
                Console.WriteLine("Chrome launched with debugging enabled.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open Chrome: {ex.Message}");
                return false;
            }

        }
    }
}
