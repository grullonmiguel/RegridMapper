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

        public static void LaunchChromeWithDebugging()
        {
            var chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            var userDataDir = @"C:\ChromeSession";
            var debuggingPort = 9222;
            var debuggerUrl = $"http://127.0.0.1:{debuggingPort}/json";

            // Start Chrome
            var processInfo = new ProcessStartInfo
            {
                FileName = chromePath,
                Arguments = $"--remote-debugging-port={debuggingPort} --user-data-dir=\"{userDataDir}\" --new-window",
                Verb = "runas", // Launch as Administrator
                UseShellExecute = true // Required for 'runas'
            };

            try
            {
                Process.Start(processInfo);
                Console.WriteLine("Chrome launched with debugging enabled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open Chrome: {ex.Message}");
            }

        }
    }
}
