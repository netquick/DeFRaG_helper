﻿using DeFRaG_Helper.Helpers;
using DeFRaG_Helper.ViewModels;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace DeFRaG_Helper
{
    internal class CreateAndUpdateDB
    {

        // Define a static HttpClient instance at the class level
        private static readonly HttpClient httpClient = CreateHttpClient();
        //method to create the database in the Appdata folder. DO ONLY USE TO CREATE A NEW DB!!!
        private static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string AppDataFolder = Path.Combine(AppDataPath, "DeFRaG_Helper");
        private static readonly string DbPath = Path.Combine(AppDataFolder, "MapData.db");


        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();

            if (AppConfig.UseUnsecureConnection.HasValue && AppConfig.UseUnsecureConnection.Value)
            {
                handler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true; // Bypass SSL certificate validation for unsecure connections
            }

            return new HttpClient(handler);
        }

        private static void CreateDB()
        { 
            if (!System.IO.Directory.Exists(AppDataFolder))
            {
                System.IO.Directory.CreateDirectory(AppDataFolder);
            }
            if (!System.IO.File.Exists(DbPath))
            {
                //Create the datebase using dbqueue
                CreateDBMaplist();
                CreateAllDB();
            }

        }

        public  static async Task UpdateDB()
        {
            try
            {
                MessageHelper.ShowMessage("Checking for Database Updates");
                // Create an instance of CreateAndUpdateDB

                // Step 1: Check the amount of pages
                int pageCount = await GetLastPageNumberAsync() + 1;

                // Step 2: Parse each page and update the database
                await ParsePagesAsync(pageCount);

                await UpdateMapDetails();
                //Update map details
                //TODO: Implement the map details update logic here

                MessageHelper.ShowMessage("Database update completed successfully");
            }
            catch (Exception ex)
            {
                // Handle any errors that might occur during the update process
                MessageHelper.Log($"An error occurred during the database update: {ex.Message}");
            }
        }



        public static void CreateDBMaplist()
        {
            // Define the operation to create the Maplist table
            Func<SqliteConnection, Task> createTableOperation = async (connection) =>
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText =
                    @"CREATE TABLE IF NOT EXISTS Maplist (
                ID INTEGER PRIMARY KEY, 
                Name TEXT, 
                LinkDetailpage TEXT, 
                Filename TEXT,
                Parsed INTEGER
            )";
                await createTableCmd.ExecuteNonQueryAsync();
            };

            // Enqueue the operation
            DbQueue.Instance.Enqueue(createTableOperation);
        }

        public static async Task AddMapToList(string name, string linkDetailPage, string filename, int parsed)
        {
            // Updated to use DbQueueInstance
            Func<SqliteConnection, Task> insertOperation = async (connection) =>
            {
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                INSERT INTO Maplist (Name, LinkDetailpage, Filename, Parsed) 
                VALUES (@Name, @LinkDetailpage, @Filename, @Parsed)";

                insertCmd.Parameters.AddWithValue("@Name", name);
                insertCmd.Parameters.AddWithValue("@LinkDetailpage", linkDetailPage);
                insertCmd.Parameters.AddWithValue("@Filename", filename);
                insertCmd.Parameters.AddWithValue("@Parsed", parsed);

                await insertCmd.ExecuteNonQueryAsync();
            };
            // Enqueue the operation using the centralized instance
            await Task.Run(() => DbQueue.Instance.Enqueue(insertOperation));
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


        //parse map pages for new maps
        private static async Task ParsePagesAsync(int pageCount)
        {


            int consecutiveExistingMaps = 0;
            List<string> existingMapLinks = new List<string>();

            // Enqueue the operation to load LinkDetailpage values
            DbQueue.Instance.Enqueue(async connection =>
            {
                var command = new SqliteCommand("SELECT LinkDetailpage FROM Maplist", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // Ensure this line is executed and adds items to the list
                        existingMapLinks.Add(reader.GetString(0));
                    }
                }
            });
            await DbQueue.Instance.WhenAllCompleted();

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

                        if (cells.Count >= 6)
                        {
                            // Assuming the second cell contains the link and name
                            var detailLinkRegex = new Regex(@"<a.*?href=['""](.*?)['""].*?>(.*?)</a>", RegexOptions.Singleline);
                            var detailLinkMatch = detailLinkRegex.Match(cells[1].Value);

                            if (detailLinkMatch.Success)
                            {
                                var detailsPageUrl = detailLinkMatch.Groups[1].Value.Trim();
                                var fullDetailsPageUrl = $"https://ws.q3df.org{detailsPageUrl}";
                                var name = detailLinkMatch.Groups[2].Value.Trim();

                                // Check if the map already exists using the loaded list
                                if (existingMapLinks.Contains(fullDetailsPageUrl))
                                {
                                    consecutiveExistingMaps++;
                                    if (consecutiveExistingMaps >= 3)
                                    {
                                        Console.WriteLine("3 consecutive maps found. Cancelling the process.");
                                        return; // Cancel the process
                                    }
                                    continue; // Skip to the next map
                                }
                                else
                                {
                                    consecutiveExistingMaps = 0; // Reset the counter if a new map is found
                                    MessageHelper.Log($"Parsing map: {fullDetailsPageUrl}");
                                }

                                // Assuming the third cell contains the filename in an <a> tag's href attribute
                                var filenameRegex = new Regex(@"<a href=""/maps/downloads/(.*?)\.pk3"">", RegexOptions.Singleline);
                                var filenameMatch = filenameRegex.Match(cells[2].Value);

                                string filename = "";
                                if (filenameMatch.Success)
                                {
                                    filename = filenameMatch.Groups[1].Value.Trim();
                                }


                                // Call AddMapToList with the extracted data
                                await AddMapToList(name, fullDetailsPageUrl, filename, 0);
                            }
                        }
                    }
                    MessageHelper.Log($"Page {i + 1} of {pageCount} processed.");

                }
            } catch (Exception ex)
            {
                MessageHelper.Log($"An error occurred during the database update: {ex.Message}");
                throw;
            }
        }

        //method to get maplist with links for all maps that have parsed = 0
        private async static Task<List<MapData>> GetMapList()
        {
            List<MapData> mapDataList = new List<MapData>();

            // Enqueue the operation to load all map data
            DbQueue.Instance.Enqueue(async connection =>
            {
                var command = new SqliteCommand("SELECT Name, LinkDetailpage, Filename, Parsed FROM Maplist", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // Create a new MapData instance for each row and add it to the list
                        var mapData = new MapData
                        {
                            Name = reader.GetString(0),
                            LinkDetailPage = reader.GetString(1),
                            Filename = reader.GetString(2),
                            Parsed = reader.GetInt32(3)
                        };

                        if (mapData.Parsed == 0 )
                        {
                            mapDataList.Add(mapData);
                        }
                        
                    } 
                    
                }
            });
            await DbQueue.Instance.WhenAllCompleted();

            return mapDataList;
        }

        private static async Task UpdateMapDetails()
        {
            // Get the list of maps with parsed = 0
            var mapList = await GetMapList(); // Ensure this is awaited
            var mapCount = mapList.Count;
            // Loop through each map and parse the details
            foreach (var map in mapList)
            {

                ///ShowMessage($"Updating Details for {map.Name}, Number of maps left: {mapCount}");
                mapCount--;
                await GetMapDetails(map, mapCount); // Await the call to ensure completion
            }
        }


        //method to parse details for individual map
        private async static Task GetMapDetails(MapData tempMap, int mapCount)
        {
            //ShowMessage($"Updating Details for {tempMap.Name}");

            //create a new map object to store the details
            Map map = new Map();
            // parse the LinkDetailPage for the map details
            map.LinkDetailpage = tempMap.LinkDetailPage;

            var url = tempMap.LinkDetailPage;
            var html = await httpClient.GetStringAsync(url);

            /// First, isolate the 'maps_table' table from the HTML content
            var tableRegex = new Regex(@"<table[^>]+id=['""]mapdetails_data_table['""][^>]*>(.*?)</table>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var tableMatch = tableRegex.Match(html);
            if (tableMatch.Success)
            {
                var tableContent = tableMatch.Groups[1].Value;

                // Regex to isolate each row in the table
                var rowRegex = new Regex(@"<tr>(.*?)</tr>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                var rows = rowRegex.Matches(tableContent);

                // Dictionary to store property names and values
                var properties = new Dictionary<string, string>();


                //Process each row and parse the property name and value
                foreach (Match row in rows)
                {
                    var tdRegex = new Regex(@"<td.*?>(.*?)</td>", RegexOptions.Singleline);
                    var tds = tdRegex.Matches(row.Value);

                    if (tds.Count == 2) // Ensure there are exactly two <td> elements
                    {
                        var propertyName = tds[0].Groups[1].Value.Trim(); // First <td> content
                        var propertyValue = tds[1].Groups[1].Value.Trim(); // Second <td> content

                        // Call the new class method
                        MapDetailProcessor.ProcessProperty(propertyName, propertyValue, map);
                    }
                }


                if (map.Name == null || map.Name == "")
                {
                    map.Name = tempMap.Name;
                }

                await DownloadImages(map, html);

                MessageHelper.ShowMessage($"Details updated and images downloaded for {map.Name}, \r\nFile: {map.Filename}, Mapname: {map.Mapname}, {mapCount} maps left");

                //update the map in the database with method UpdateOrAddMap in MapViewModel
                var mapViewModel = await MapViewModel.GetInstanceAsync();

                //Update or add map to the database
                await mapViewModel.UpdateOrAddMap(map);

                //set the map as parsed
                await SetMapParsed(tempMap);


            }
        }


        //Seperate method to download images
        private static async Task DownloadImages(Map map, string html) 
        {
            //check in appconfig if images should be downloaded
            if (AppConfig.DownloadImagesOnUpdate == false)
            {
                return;
            }

            var imageUrls = ExtractImageUrls(html);
            for(int i = 0; i < imageUrls.Count; i++)
            {
                var imageUrl = imageUrls[i];
                // Use the map's name as the base file name for the image
                var fileName = Path.GetFileNameWithoutExtension(map.Name);
                var folderPath = GetImagePath(imageUrl);
                var filePath = Path.Combine(folderPath, $"{fileName}_{i}.jpg");

                // Download the image asynchronously
                await DownloadImageAsync(imageUrl, "https://ws.q3df.org");
            }
        }











        public static string DecodeHtmlString(string htmlString)
        {
            return System.Net.WebUtility.HtmlDecode(htmlString);
        }


        public static async Task SetMapParsed(MapData map)
        {
            DbQueue.Instance.Enqueue(async connection =>
            {
                using (var command = new SqliteCommand(@"UPDATE Maplist 
SET Parsed = 1 
WHERE LinkDetailpage = @LinkDetailpage", connection))
                {
                    command.Parameters.AddWithValue("@LinkDetailpage", map.LinkDetailPage);
                    await command.ExecuteNonQueryAsync();
                }
            });
            await Task.CompletedTask;
        }
   
        public static List<string> ExtractImageUrls(string html)
        {
            var imageUrls = new List<string>();
            var regex = new Regex(@"<img id=""mapdetails_levelshot""[^>]+src=""([^""]+)""[^>]+data-slide_count=""(\d+)""(.*?)>", RegexOptions.Singleline);
            var match = regex.Match(html);

            if (match.Success)
            {
                // Add the main image src
                imageUrls.Add(match.Groups[1].Value);

                // Extract additional images from data-slide_srcsetX attributes
                int slideCount = int.Parse(match.Groups[2].Value);
                string dataAttributes = match.Groups[3].Value;

                for (int i = 1; i <= slideCount; i++)
                {
                    var slideRegex = new Regex($@"data-slide_srcset{i}=""([^""]+)""");
                    var slideMatch = slideRegex.Match(dataAttributes);
                    if (slideMatch.Success)
                    {
                        //MessageHelper.Log($"Found slide {i}: {slideMatch.Groups[1].Value}");
                        imageUrls.Add(slideMatch.Groups[1].Value);
                    }
                }
            }

            return imageUrls;
        }


        //public static async Task DownloadImageAsync(string imageUrl, string baseUrl)
        //{
        //    // Construct the full URL for the image
        //    string fullUrl = new Uri(new Uri(baseUrl), imageUrl).ToString();
        //    // Extract the original file name from the image URL
        //    string fileName = Path.GetFileName(new Uri(fullUrl).AbsolutePath);
        //    // Determine the local path for saving the image
        //    string folderPath = GetImagePath(imageUrl);
        //    string filePath = Path.Combine(folderPath, fileName);

        //    using (var client = httpClient)
        //    {
        //        var response = await client.GetAsync(fullUrl);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var imageBytes = await response.Content.ReadAsByteArrayAsync();
        //            await File.WriteAllBytesAsync(filePath, imageBytes);
        //        }
        //    }
        //}
        //public static async Task DownloadImageAsync(string imageUrl, string baseUrl)
        //{
        //    // Construct the full URL for the image
        //    string fullUrl = new Uri(new Uri(baseUrl), imageUrl).ToString();
        //    // Extract the original file name from the image URL
        //    string fileName = Path.GetFileName(new Uri(fullUrl).AbsolutePath);
        //    // Determine the local path for saving the image
        //    string folderPath = GetImagePath(imageUrl);
        //    string filePath = Path.Combine(folderPath, fileName);

        //    var response = await httpClient.GetAsync(fullUrl);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var imageBytes = await response.Content.ReadAsByteArrayAsync();
        //        await File.WriteAllBytesAsync(filePath, imageBytes);
        //    }
        //}
        public static async Task DownloadImageAsync(string imageUrl, string baseUrl)
        {
            // Construct the full URL for the image
            string fullUrl = new Uri(new Uri(baseUrl), imageUrl).ToString();
            // Extract the original file name from the image URL
            string fileName = Path.GetFileName(new Uri(fullUrl).AbsolutePath);
            // Determine the local path for saving the image
            string folderPath = GetImagePath(imageUrl);
            string filePath = Path.Combine(folderPath, fileName);

            // Ensure the directory exists
            Directory.CreateDirectory(folderPath);

            try
            {
                var response = await httpClient.GetAsync(fullUrl);
                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(filePath, imageBytes);
                }
                else
                {
                    // Log or handle the unsuccessful response
                    SimpleLogger.Log($"Failed to download {fullUrl}. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                SimpleLogger.Log($"Exception occurred while downloading {fullUrl}: {ex.Message}");
            }
        }



        public static string GetImagePath(string imageUrl)
        {
            // Get the base path for app data
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string baseFolder = Path.Combine(appDataPath, "DeFRaG_Helper", "PreviewImages");

            // Determine the subfolder based on the imageUrl content
            string folderName = "";
            if (imageUrl.Contains("/levelshots/"))
            {
                folderName = "Levelshots";
            }
            else if (imageUrl.Contains("/topviews/"))
            {
                folderName = "Topviews";
            }
            else if (imageUrl.Contains("/authorshots/") || imageUrl.Contains("/screenshots/")) // Adjust based on actual URL structure
            {
                folderName = "Screenshots";
            }

            // Construct the full path and ensure the directory exists
            string fullPath = Path.Combine(baseFolder, folderName);
            Directory.CreateDirectory(fullPath); // This will create the directory if it does not exist

            return fullPath;
        }


        public async static void CreateAllDB()
        {
            CreateDBMaps();
            FillGameTypeTable();
            FillModTable();
            FillWeaponTable();
            FillFunctionable();
            FillItemTable();
        }

        private static void CreateDBMaps()
        {
            Func<SqliteConnection, Task> operation = async (connection) =>
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText =
                    @"CREATE TABLE Maps (
                ID INTEGER PRIMARY KEY, 
                Name TEXT, 
                Mapname TEXT, 
                Filename TEXT, 
                Releasedate TEXT, 
                Author TEXT, 
                Mod TEXT, 
                Size DOUBLE,
                Physics INT,
                Hits INTEGER, 
                LinkDetailpage TEXT,  
                Style TEXT, 
                LinksOnlineRecordsQ3DFVQ3 TEXT, 
                LinksOnlineRecordsQ3DFCPM TEXT, 
                LinksOnlineRecordsRacingVQ3 TEXT, 
                LinksOnlineRecordsRacingCPM TEXT, 
                LinkDemosVQ3 TEXT, 
                LinkDemosCPM TEXT, 
                DependenciesTextures TEXT, 
                Screenshot TEXT, 
                Levelshot TEXT, 
                Topview TEXT 
                )";
                await createTableCmd.ExecuteNonQueryAsync();

                // Create Players table
                var createPlayersTableCmd = connection.CreateCommand();
                createPlayersTableCmd.CommandText =
                    @"CREATE TABLE GameType (
                GameTypeID INTEGER PRIMARY KEY,
                GameType TEXT
                )";

                await createTableCmd.ExecuteNonQueryAsync();


                // Create MapGameType junction table
                var createMapGameTypeTableCmd = connection.CreateCommand();
                createMapGameTypeTableCmd.CommandText =
                    @"CREATE TABLE MapGameType (
                MapID INTEGER,
                GameTypeID INTEGER,
                PRIMARY KEY (MapID, GameTypeID),
                FOREIGN KEY (MapID) REFERENCES Maps(ID),
                FOREIGN KEY (GameTypeID) REFERENCES GameType(GameTypeID)
                )";
                await createTableCmd.ExecuteNonQueryAsync();

                // Create Mod table
                var createModTableCmd = connection.CreateCommand();
                createModTableCmd.CommandText =
                    @"CREATE TABLE Mod (
                ModID INTEGER PRIMARY KEY,
                Mod TEXT
                )";
                await createTableCmd.ExecuteNonQueryAsync();

                // Create MapMod junction table
                var createMapModTableCmd = connection.CreateCommand();
                createMapModTableCmd.CommandText =
                    @"CREATE TABLE MapMod (
                MapID INTEGER,
                ModID INTEGER,
                PRIMARY KEY (MapID, ModID),
                FOREIGN KEY (MapID) REFERENCES Maps(ID),
                FOREIGN KEY (ModID) REFERENCES Mod(ModID)
                )";
                await createTableCmd.ExecuteNonQueryAsync();

                //Create weapon table
                var createWeaponTableCmd = connection.CreateCommand();
                createWeaponTableCmd.CommandText =
                    @"CREATE TABLE Weapon (
                WeaponID INTEGER PRIMARY KEY,
                Weapon TEXT
                )";
                await createTableCmd.ExecuteNonQueryAsync();

                // Create MapWeapon junction table
                var createMapWeaponTableCmd = connection.CreateCommand();
                createMapWeaponTableCmd.CommandText =
                    @"CREATE TABLE MapWeapon (
                MapID INTEGER,
                WeaponID INTEGER,
                PRIMARY KEY (MapID, WeaponID),
                FOREIGN KEY (MapID) REFERENCES Maps(ID),
                FOREIGN KEY (WeaponID) REFERENCES Weapon(WeaponID)
                )";
                await createTableCmd.ExecuteNonQueryAsync();

                //Create item table
                var createItemTableCmd = connection.CreateCommand();
                createItemTableCmd.CommandText =
                    @"CREATE TABLE Item (
                ItemID INTEGER PRIMARY KEY,
                Item TEXT
                )";
                await createTableCmd.ExecuteNonQueryAsync();

                // Create MapItem junction table
                var createMapItemTableCmd = connection.CreateCommand();

                createMapItemTableCmd.CommandText =
                    @"CREATE TABLE MapItem (
                MapID INTEGER,
                ItemID INTEGER,
                PRIMARY KEY (MapID, ItemID),
                FOREIGN KEY (MapID) REFERENCES Maps(ID),
                FOREIGN KEY (ItemID) REFERENCES Item(ItemID)
                )";
                await createTableCmd.ExecuteNonQueryAsync();

                // Create Functions table
                var createFunctionsTableCmd = connection.CreateCommand();
                createFunctionsTableCmd.CommandText =
                    @"CREATE TABLE Function (
                FunctionID INTEGER PRIMARY KEY,
                Function TEXT
                )";
                await createTableCmd.ExecuteNonQueryAsync();

                // Create MapFunction junction table
                var createMapFunctionTableCmd = connection.CreateCommand();
                createMapFunctionTableCmd.CommandText =
                    @"CREATE TABLE MapFunction (
                MapID INTEGER,
                FunctionID INTEGER,
                PRIMARY KEY (MapID, FunctionID),
                FOREIGN KEY (MapID) REFERENCES Maps(ID),
                FOREIGN KEY (FunctionID) REFERENCES Functions(FunctionID)
                )";
                await createTableCmd.ExecuteNonQueryAsync();
            };
            DbQueue.Instance.Enqueue(operation);

        }

        //method to fill the optional tables
        private static void FillGameTypeTable()
        {
            Func<SqliteConnection, Task> operation = async (connection) =>
            {
                // Fill GameType table
                var insertGameTypeCmd = connection.CreateCommand();
                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('CTF')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('SP')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('FFA')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('TDM')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('Tournament')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('OneFlag')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('Overload')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('Harvester')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('Classic CTF')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('Capture Strike')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('Domination')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('Lastman survivor')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('Mission')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('Reverse CTF')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('Team Survivor')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('Follow the leader')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('Capture and hold')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('Bomb and defuse')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('CTF Urban Terror')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO GameType (GameType) VALUES ('TDM Urban Terror')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

            };
            DbQueue.Instance.Enqueue(operation);

        }

        private static void FillModTable()
        {
            Func<SqliteConnection, Task> operation = async (connection) =>
            {
                // Fill Mod table
                var insertGameTypeCmd = connection.CreateCommand();
                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Defrag')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Defrag Freestyle')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Weapons Factory Arena')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Urban Terror')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Fortress')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Threewave')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Navy Seals')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Team Arena')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Rocket Arena')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Rally')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('True Combat')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Reaction Quake 3')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Tremulous')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('FreakBall')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Jailbreak')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('ProBall')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Domination')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('World of Padman')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Jailbreak: Prisoners of War')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Bid for Power')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Western Quake 3')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('MatchMod 2000')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Challenge Pro Mode Arena')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('Sidrial')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Mod (Mod) VALUES ('PainKeep Arena')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

            };
            DbQueue.Instance.Enqueue(operation);
        }

        private static void FillWeaponTable()
        {
            Func<SqliteConnection, Task> operation = async (connection) =>
            {
                // Fill GameType table
                var insertGameTypeCmd = connection.CreateCommand();
                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Weapon (Weapon) VALUES ('Gauntlet')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Weapon (Weapon) VALUES ('Machinegun')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Weapon (Weapon) VALUES ('Shotgun')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Weapon (Weapon) VALUES ('Grenade Launcher')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Weapon (Weapon) VALUES ('Rocket Launcher')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Weapon (Weapon) VALUES ('Lightning Gun')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Weapon (Weapon) VALUES ('Railgun')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Weapon (Weapon) VALUES ('Plasma Gun')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Weapon (Weapon) VALUES ('BFG10K')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Weapon (Weapon) VALUES ('Grappling hook')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Weapon (Weapon) VALUES ('Chaingun')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Weapon (Weapon) VALUES ('Nailgun')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Weapon (Weapon) VALUES ('Prox Launcher')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();
            };
            DbQueue.Instance.Enqueue(operation);
        }

        private static void FillFunctionable()
        {
            Func<SqliteConnection, Task> operation = async (connection) =>
            {

                // Fill GameType table
                var insertGameTypeCmd = connection.CreateCommand();
                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Door/Gate')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Button')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Teleporter')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Jumppad/Launchramp')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Moving Object/platform')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Shooter Grenade')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Shooter Rocket')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Shooter Plasma')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Slick')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Lava')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Water')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Slime')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Fog')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Breakable')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Ambient Sound')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Function (Function) VALUES ('Timer')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();
            };
            DbQueue.Instance.Enqueue(operation);
        }

        private static void FillItemTable()
        {
            Func<SqliteConnection, Task> operation = async (connection) =>
            {
                // Fill GameType table
                var insertGameTypeCmd = connection.CreateCommand();
                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Body Armor Red')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Body Armor Yellow')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Body Armor Green')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Shard Armor')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Battle Suit')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Flight')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Haste')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Health')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Health Small')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Health Large')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Mega Health')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Quad')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Regeneration')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Invisibility')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Medikit')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Personal Teleporter')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Ammo Regen')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Scout')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Guard')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Doubler')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Kamikaze')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();

                insertGameTypeCmd.CommandText =
                    @"INSERT INTO Item (Item) VALUES ('Invulnerability')";
                await insertGameTypeCmd.ExecuteNonQueryAsync();
            };
            DbQueue.Instance.Enqueue(operation);
        }





    }
}
