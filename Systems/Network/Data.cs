using System.IO;
using System.Net.Http;

namespace BazthalLib.Systems.Network
{
    public class Data
    {
        /// <summary>
        /// Downloads a file from the specified URL and saves it to the given path.
        /// </summary>
        /// <remarks>This method checks for network availability before attempting to download the file.
        /// If the network is unavailable, the method logs a message and does not attempt the download. If the download
        /// fails due to an unsuccessful HTTP response, a log entry is created with the status code.</remarks>
        /// <param name="url">The URL of the file to download. Must be a valid HTTP or HTTPS URL.</param>
        /// <param name="path">The local file path where the downloaded file will be saved. The path must be writable.</param>
        public static void DownloadFile(string url, string path)
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {

                HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.Add("Accept", "text/html, application/xhtml+xml, */*");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");

                using var response = httpClient.GetAsync(url).Result;
                
                    if (response.IsSuccessStatusCode)
                    {
                        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                            response.Content.CopyToAsync(fileStream).Wait();
                    }
                    else
                    {
                        DebugUtils.Log("Networking", "DowloadFile", $"Failed to download file. Status code: {response.StatusCode}");
                    }
                
            }
            else
            {
                DebugUtils.Log("Networking", "DownloadFile", "No network connection detected. Please check your network connection and try again.");
            }
        }
    }
}
