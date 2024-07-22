using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace DeFRaG_Helper.Helpers
{
    public class GitHubReleaseChecker
    {
        private readonly HttpClient _httpClient;


        public GitHubReleaseChecker()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
        }

        public async Task CheckForNewReleaseAsync(string owner, string repo)
        {
            string latestReleaseUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            MessageHelper.ShowMessage($"Checking for new release: {latestReleaseUrl}");
            try
            {
                var response = await _httpClient.GetAsync(latestReleaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(responseContent))
                    {
                        JsonElement root = doc.RootElement;
                        string tagName = root.GetProperty("tag_name").GetString();
                        string releaseName = root.GetProperty("name").GetString();
                        string downloadUrl = root.GetProperty("assets")[0].GetProperty("browser_download_url").GetString();

                        Version latestVersion = new Version(tagName.TrimStart('v'));
                        Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

                        if (latestVersion > currentVersion)
                        {
                            MessageHelper.ShowMessage($"Newer version available: {releaseName} (Tag: {tagName})");
                            MessageHelper.Log($"Download URL: {downloadUrl}");
                            await DownloadAndUpdateLatestRelease(owner, repo);
                        }
                        else
                        {
                            MessageHelper.ShowMessage("Your application is up to date.");
                        }
                    }
                }
                else
                {
                    MessageHelper.Log($"Failed to fetch latest release. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageHelper.Log($"Error checking for release: {ex.Message}");
            }
        }

        public async Task DownloadAndUpdateLatestRelease(string owner, string repo)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"https://api.github.com/repos/{owner}/{repo}/releases/latest");
                using (JsonDocument doc = JsonDocument.Parse(response))
                {
                    JsonElement root = doc.RootElement;
                    string downloadUrl = root.GetProperty("assets")[0].GetProperty("browser_download_url").GetString();
                    string tempPath = Path.GetTempFileName();

                    // Use Downloader to download the file
                    await Downloader.DownloadFileAsyncWithConnection(downloadUrl, tempPath, null, _httpClient);

                    // Verify the downloaded file exists and optionally check its integrity
                    if (File.Exists(tempPath))
                    {
                        // Optional: Verify file integrity (e.g., by checking file size, computing and comparing file hash, etc.)

                        // Schedule the update (simple example: rename on next launch)
                        string appExecutablePath = Assembly.GetExecutingAssembly().Location;
                        string batchCommands = $"timeout /t 5 /nobreak > NUL & move /y \"{tempPath}\" \"{appExecutablePath}\" > update.log 2>&1 & start \"\" \"{appExecutablePath}\" & del \"%~f0\" >> update.log 2>&1";
                        File.WriteAllText("update.bat", batchCommands);
                        Process.Start("update.bat");
                        MessageHelper.Log("Batch script created and executed for update.");

                        // Exit the application to allow the update to proceed
                        Environment.Exit(0);
                    }
                    else
                    {
                        MessageHelper.Log("Download failed or file does not exist.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.Log($"Error downloading or updating release: {ex.Message}");
            }
        }

        public async Task<string> DownloadLatestReleaseAssetAsync(string owner, string repo, string destinationPath)
        {
            string downloadedFilePath = null; // This will hold the path of the downloaded file

            try
            {
                string latestReleaseUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
                var response = await _httpClient.GetAsync(latestReleaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(responseContent))
                    {
                        JsonElement root = doc.RootElement;
                        JsonElement assets = root.GetProperty("assets");

                        string downloadUrl = null;
                        foreach (JsonElement asset in assets.EnumerateArray())
                        {
                            string assetName = asset.GetProperty("name").GetString();
                            // Check if the asset's name contains the specific pattern for Windows
                            if (assetName.Contains("windows-x86_64.zip"))
                            {
                                downloadUrl = asset.GetProperty("browser_download_url").GetString();
                                break; // Stop searching once we find the correct asset
                            }
                        }

                        if (downloadUrl != null)
                        {
                            // Extract the filename from the download URL
                            string filename = downloadUrl.Split('/').Last();
                            downloadedFilePath = Path.Combine(destinationPath, filename);

                            // Use Downloader to download the file
                            await Downloader.DownloadFileAsyncWithConnection(downloadUrl, downloadedFilePath, null, _httpClient);

                            // Optionally, verify the downloaded file exists and check its integrity
                            if (File.Exists(downloadedFilePath))
                            {
                                MessageHelper.Log("Latest release asset downloaded successfully.");
                            }
                            else
                            {
                                MessageHelper.Log("Download failed or file does not exist.");
                                downloadedFilePath = null; // Reset to null as the download was not successful
                            }
                        }
                        else
                        {
                            MessageHelper.Log("No matching asset found for Windows.");
                        }
                    }
                }
                else
                {
                    MessageHelper.Log($"Failed to fetch latest release. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageHelper.Log($"Error downloading latest release asset: {ex.Message}");
                downloadedFilePath = null; // Reset to null due to an exception
            }

            return downloadedFilePath; // Return the path of the downloaded file or null
        }







    }

    // Usage
    class Program
    {
        static async Task Main(string[] args)
        {
            var releaseChecker = new GitHubReleaseChecker();
            await releaseChecker.CheckForNewReleaseAsync("owner-name", "repo-name");
        }
    }
}
