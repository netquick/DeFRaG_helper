using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace DeFRaG_Helper.Helpers
{
    class UpdateDlCounts
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task UpdateDownloadCounts(int pageCount)
        {
            try
            {
                for (int i = 0; i < pageCount; i++)
                {
                    var url = $"https://ws.q3df.org/maps/?map=&show=50&page={i}";
                    var html = await httpClient.GetStringAsync(url);

                    // First, isolate the 'maps_table' table from the HTML content
                    var tableRegex = new Regex(@"<table[^>]+id=['""]maps_table['""][^>]*>(.*?)</table>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    var tableMatch = tableRegex.Match(html);
                    if (!tableMatch.Success) continue; // Skip if the table isn't found

                    var tableContent = tableMatch.Groups[1].Value;

                    // Now, parse each row within the isolated table content
                    var rowRegex = new Regex(@"<tr\s+class=['""]?[^'""]*['""]?>(.*?)</tr>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    var rows = rowRegex.Matches(tableContent);

                    foreach (Match row in rows)
                    {
                        var cellRegex = new Regex(@"<td.*?>(.*?)</td>", RegexOptions.Singleline);
                        var cells = cellRegex.Matches(row.Value);

                        if (cells.Count >= 10)
                        {
                            // Assuming the second cell contains the link and name
                            var detailLinkRegex = new Regex(@"<a.*?href=['""](.*?)['""].*?>(.*?)</a>", RegexOptions.Singleline);
                            var detailLinkMatch = detailLinkRegex.Match(cells[1].Value);

                            if (detailLinkMatch.Success)
                            {
                                var detailsPageUrl = detailLinkMatch.Groups[1].Value.Trim();
                                var fullDetailsPageUrl = $"https://ws.q3df.org{detailsPageUrl}";

                                // Assuming the tenth cell contains the hits count
                                var hitsRegex = new Regex(@"\s*(\d+)\s*", RegexOptions.Singleline);
                                var hitsMatch = hitsRegex.Match(cells[9].Value);

                                if (hitsMatch.Success)
                                {
                                    int hitsCount = int.Parse(hitsMatch.Groups[1].Value.Trim());

                                    // Update the database with the new hits count
                                    await UpdateMapHitsCount(fullDetailsPageUrl, hitsCount);
                                }
                            }
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                MessageHelper.Log($"An error occurred during the download count update: {ex.Message}");
                throw;
            }
            MessageHelper.ShowMessage("Download count update completed.");
        }
        private static async Task UpdateMapHitsCount(string detailsPageUrl, int hitsCount)
        {
            DbQueue.Instance.Enqueue(async connection =>
            {
                var command = new SqliteCommand("UPDATE Maps SET Hits = @Hits WHERE LinkDetailpage = @LinkDetailpage", connection);
                command.Parameters.AddWithValue("@Hits", hitsCount);
                command.Parameters.AddWithValue("@LinkDetailpage", detailsPageUrl);
                await command.ExecuteNonQueryAsync();
            });
            await DbQueue.Instance.WhenAllCompleted();
        }
        private static async Task UpdateMapDownloadCount(string detailsPageUrl, int downloadCount)
        {
            DbQueue.Instance.Enqueue(async connection =>
            {
                var command = new SqliteCommand("UPDATE Maplist SET DownloadCount = @DownloadCount WHERE LinkDetailpage = @LinkDetailpage", connection);
                command.Parameters.AddWithValue("@DownloadCount", downloadCount);
                command.Parameters.AddWithValue("@LinkDetailpage", detailsPageUrl);
                await command.ExecuteNonQueryAsync();
            });
            await DbQueue.Instance.WhenAllCompleted();
        }
    }
}
