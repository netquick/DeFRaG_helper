using System.Net.Http;
using System.Text.Json;

namespace DeFRaG_Helper
{
    internal class DemoParser
    {
        private static readonly HttpClient client = new HttpClient();

        public static string GetDemoLink(string mapName)
        {
            return $"http://95.31.6.66/~/api/get_file_list?uri=/demos/{mapName[0]}/{mapName}/";
        }

        public static async Task<List<DemoItem>> GetDemoLinksAsync(string demoLink)
        {
            try
            {
                string jsonResponse = await client.GetStringAsync(demoLink);
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(jsonResponse);
                List<DemoItem> demoItems = new List<DemoItem>();
                if (apiResponse?.list != null)
                {
                    foreach (var item in apiResponse.list)
                    {
                        demoItems.Add(new DemoItem
                        {
                            Name = item.n,
                            Date = DateTime.Parse(item.m),
                            Size = item.s,
                        });
                    }
                }
                return demoItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching or parsing demo links: {ex.Message}");
                return new List<DemoItem>();
            }
        }

        // Define classes to match the JSON structure returned by the API
        public class ApiResponse
        {
            public bool can_archive { get; set; }
            public bool can_upload { get; set; }
            public bool can_delete { get; set; }
            public bool can_overwrite { get; set; }
            public bool can_comment { get; set; }
            public List<DemoItemJson> list { get; set; }
        }
    }
}
