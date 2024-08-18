using DeFRaG_Helper.Helpers;
using DeFRaG_Helper.ViewModels;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace DeFRaG_Helper
{
    public class BackgroundTaskRunner
    {
        private static readonly HttpClient httpClient = CreateHttpClient();

        private TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
        public Task BackgroundTask => _tcs.Task;
        private bool _isInitialized = false;

        public BackgroundTaskRunner()
        {
            Initialize();
        }

        private async void Initialize()
        {
            if (_isInitialized) return;

            var viewModel = await MapViewModel.GetInstanceAsync();
           // viewModel.DataLoaded += ViewModel_DataLoaded;
            _isInitialized = true;
        }

        private async void ViewModel_DataLoaded(object sender, EventArgs e)
        {
            // Unsubscribe to prevent multiple executions
            ((MapViewModel)sender).DataLoaded -= ViewModel_DataLoaded;

            // Start the background task
            await RunTaskAsync();

            // Signal that the task is completed
            _tcs.SetResult(true);
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();

            if (AppConfig.UseUnsecureConnection.HasValue && AppConfig.UseUnsecureConnection.Value)
            {
                handler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true; // Bypass SSL certificate validation for unsecure connections
            }

            return new HttpClient(handler);
        }
        public async Task RunTaskAsync()
        {
            //ShowMessage("Starting background task");

            GitHubReleaseChecker checkGit = new GitHubReleaseChecker();
            await checkGit.CheckForNewReleaseAsync("netquick", "DeFRaG_Helper").ConfigureAwait(false);

            await CheckInstallState();

            //here can call the count update
            int pageCount = await GetLastPageNumberAsync() + 1;
            MessageHelper.ShowMessage($"Updating hit counts for maps");
            await UpdateDlCounts.UpdateDownloadCounts(pageCount);


        }
        private static async Task<int> GetLastPageNumberAsync()
        {
            var url = "https://ws.q3df.org/maps/?map=&show=50&page=0";
            var html = await httpClient.GetStringAsync(url);

            // Regular expression to find the 'Last page' link. This pattern might need adjustments.
            var regex = new Regex(@"<a\s+href=""/maps/\?map=&amp;show=50&amp;page=(\d+)""\s+title=""Last page"".*?>", RegexOptions.IgnoreCase);
            var match = regex.Match(html);

            if (match.Success)
            {
                var pageParam = match.Groups[1].Value; // This captures the page number directly
                if (int.TryParse(pageParam, out int lastPageNumber))
                {
                    return lastPageNumber;
                }
            }

            throw new Exception("Unable to find the last page number.");
        }
        private async Task CheckInstallState()
        {
            MessageHelper.ShowMessage($"Checking install state for maps in Game directory");
            var viewModel = await MapViewModel.GetInstanceAsync();
            int totalMaps = viewModel.Maps.Count;
            int processedMaps = 0;

            int countChanged = 0;
            foreach (var map in viewModel.Maps)
            {
                var mapPath = AppConfig.GameDirectoryPath + "\\defrag\\" + map.Filename;
                var archivePath = AppConfig.GameDirectoryPath + "\\archive\\" + map.Filename;
                var mapChanged = false;

                bool existsInDefrag = System.IO.File.Exists(mapPath);
                bool existsInArchive = System.IO.File.Exists(archivePath);

                // Check if the map exists in the defrag directory
                if (existsInDefrag)
                {
                    if (map.IsInstalled == 0) { map.IsInstalled = 1; mapChanged = true; }
                    if (map.IsDownloaded == 0) { map.IsDownloaded = 1; mapChanged = true; }
                }

                // Check if the map exists in the archive directory
                if (existsInArchive && !existsInDefrag)
                {
                    if (map.IsDownloaded == 0) { map.IsDownloaded = 1; mapChanged = true; }
                    if (map.IsInstalled != 0) { map.IsInstalled = 0; mapChanged = true; }
                }

                // If the map doesn't exist in either directory, set both to 0
                if (!existsInDefrag && !existsInArchive)
                {
                    if (map.IsInstalled != 0) { map.IsInstalled = 0; mapChanged = true; }
                    if (map.IsDownloaded != 0) { map.IsDownloaded = 0; mapChanged = true; }
                }

                // Optionally, update the map in the ViewModel if it changed
                if (mapChanged)
                {
                    //ShowMessage($"Updating map flags for {map.Mapname}");
                    countChanged++;
                    await viewModel.UpdateMapFlagsAsync(map);
                }

                processedMaps++;

                // Update UI only after every 10 maps processed or if it's the last map
                if (processedMaps % 10 == 0 || processedMaps == totalMaps)
                {
                    double progress = (double)processedMaps / totalMaps * 100;
                    App.Current.Dispatcher.Invoke(() => { MainWindow.Instance.UpdateProgressBar(progress); });
                }
            }
            MessageHelper.ShowMessage($"Checked {processedMaps} maps, {countChanged} maps changed");
        }




    }


}
