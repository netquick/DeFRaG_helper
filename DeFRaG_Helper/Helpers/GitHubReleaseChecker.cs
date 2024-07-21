using System.IO;
using System.Net.Http;
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

                    MessageHelper.Log($"Latest Release: {releaseName} (Tag: {tagName})");
                    MessageHelper.Log($"Download URL: {downloadUrl}");

                    // Example: Download the asset
                    //await DownloadReleaseAssetAsync(downloadUrl, "path_to_save_asset");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for release: {ex.Message}");
            }
        }


        private async Task DownloadReleaseAssetAsync(string downloadUrl, string filePath)
        {
            var response = await _httpClient.GetAsync(downloadUrl);
            using (var fs = new FileStream(filePath, FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(fs);
            }
            Console.WriteLine("Download completed.");
        }

        private async Task DownloadAndUpdateLatestRelease()
        {

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
