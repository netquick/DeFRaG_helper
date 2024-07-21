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
            // Uncomment and set your token here for authenticated requests
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "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");
        }

        public async Task CheckForNewReleaseAsync(string owner, string repo)
        {
            string latestReleaseUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            MessageHelper.ShowMessage($"Checking for new release: {latestReleaseUrl}");
            try
            {
                _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request"); // GitHub API requires a user-agent

                var response = await _httpClient.GetStringAsync(latestReleaseUrl);
                using (JsonDocument doc = JsonDocument.Parse(response))
                {
                    JsonElement root = doc.RootElement;
                    string tagName = root.GetProperty("tag_name").GetString();
                    string releaseName = root.GetProperty("name").GetString();
                    string downloadUrl = root.GetProperty("assets")[0].GetProperty("browser_download_url").GetString();

                    // Assuming tagName is in the format "v1.0.0"
                    Version latestVersion = new Version(tagName.TrimStart('v'));
                    Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

                    if (latestVersion > currentVersion)
                    {
                        MessageHelper.ShowMessage($"Newer version available: {releaseName} (Tag: {tagName})");
                        MessageHelper.Log($"Download URL: {downloadUrl}");
                        // Consider calling DownloadReleaseAssetAsync here
                        await DownloadAndUpdateLatestRelease(owner, repo);
                    }
                    else
                    {
                        MessageHelper.ShowMessage("Your application is up to date.");
                    }
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
                    await Downloader.DownloadFileAsync(downloadUrl, tempPath, null);

                    // Verify the downloaded file exists and optionally check its integrity
                    if (File.Exists(tempPath))
                    {
                        // Optional: Verify file integrity (e.g., by checking file size, computing and comparing file hash, etc.)

                        // Schedule the update (simple example: rename on next launch)
                        File.WriteAllText("update.bat", $"timeout /t 5 /nobreak > NUL & move /y \"{tempPath}\" \"{AppDomain.CurrentDomain.FriendlyName}\" & start \"\" \"{AppDomain.CurrentDomain.FriendlyName}\" & del \"%~f0\"");
                        Process.Start("update.bat");

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
