using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace DeFRaG_Helper
{
    internal class DbActions
    {
        //initialize dbqueue
        private static DbActions _instance;
        private static readonly object _lock = new object();


        private static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string AppDataFolder = System.IO.Path.Combine(AppDataPath, "DeFRaG_Helper");
        private static readonly string DbPath = System.IO.Path.Combine(AppDataFolder, "MapData.db");
        private static readonly string connectionString = $"Data Source={DbPath};";
        private DbActions() { }

        // Public static method to get the instance
        public static DbActions Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DbActions();
                    }
                    return _instance;
                }
            }
        }

        public async Task AddMap(Map map)
        {
            int newMapId = -1; // Variable to store the new map ID


            DbQueue.Instance.Enqueue(async connection =>
            {
                using (var command = new SqliteCommand(@"INSERT INTO Maps 
        (Name, Mapname, Filename, Releasedate, Author, Mod, Size, Physics, Hits, LinkDetailpage, Style, LinksOnlineRecordsQ3DFVQ3, LinksOnlineRecordsQ3DFCPM, LinksOnlineRecordsRacingVQ3, LinksOnlineRecordsRacingCPM, LinkDemosVQ3, LinkDemosCPM, DependenciesTextures) 
        VALUES 
        (@Name, @Mapname, @Filename, @Releasedate, @Author, @Mod, @Size, @Physics, @Hits, @LinkDetailpage, @Style, @LinksOnlineRecordsQ3DFVQ3, @LinksOnlineRecordsQ3DFCPM, @LinksOnlineRecordsRacingVQ3, @LinksOnlineRecordsRacingCPM, @LinkDemosVQ3, @LinkDemosCPM, @DependenciesTextures  ); select last_insert_rowid();", connection))
                {
                    command.Parameters.AddWithValue("@Name", map.Name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Mapname", map.Mapname ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Filename", map.Filename ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Releasedate", map.Releasedate ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Author", map.Author ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Mod", map.GameType ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Size", map.Size != -1 ? (object)map.Size : DBNull.Value);
                    command.Parameters.AddWithValue("@Physics", map.Physics != -1 ? (object)map.Physics : DBNull.Value);
                    command.Parameters.AddWithValue("@Hits", map.Hits != -1 ? (object)map.Hits : DBNull.Value);
                    command.Parameters.AddWithValue("@LinkDetailpage", map.LinkDetailpage ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Style", map.Style ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LinksOnlineRecordsQ3DFVQ3", map.LinksOnlineRecordsQ3DFVQ3 ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LinksOnlineRecordsQ3DFCPM", map.LinksOnlineRecordsQ3DFCPM ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LinksOnlineRecordsRacingVQ3", map.LinksOnlineRecordsRacingVQ3 ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LinksOnlineRecordsRacingCPM", map.LinksOnlineRecordsRacingCPM ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LinkDemosVQ3", map.LinkDemosVQ3 ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LinkDemosCPM", map.LinkDemosCPM ?? (object)DBNull.Value); 
                    command.Parameters.AddWithValue("@DependenciesTextures", map.Dependencies ?? (object)DBNull.Value);


                    newMapId = Convert.ToInt32(await command.ExecuteScalarAsync());
                    //await command.ExecuteNonQueryAsync();
                }
            });
            await Task.CompletedTask;




            //we need write Weapons, Items and Function to the corresponding tables
            //Weapons
            // Assuming 'MapWeapons' table has columns 'MapId' and 'WeaponId'
            // 'map.Id' should be the identifier of the map, which you might need to retrieve or have available
            // This example also assumes you have a method to get the weapon's ID by its name

            // Assuming 'map.Id' is the identifier of the current map and 'map.Weapons' is a list of weapon names for the map
            if (map.Weapons != null)
            {
                foreach (var weaponName in map.Weapons)
                {
                    DbQueue.Instance.Enqueue(async connection =>
                    {
                        // First, get the WeaponID from the 'Weapons' table
                        int weaponId = -1;
                        using (var command = new SqliteCommand("SELECT WeaponID FROM Weapon WHERE Weapon = @WeaponName", connection))
                        {
                            command.Parameters.AddWithValue("@WeaponName", weaponName);
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    weaponId = reader.GetInt32(0); // Assuming 'WeaponID' is the first column
                                }
                            }
                        }

                        // Check if the map-weapon link already exists in 'MapWeapon'
                        bool exists = false;
                        using (var checkCommand = new SqliteCommand("SELECT COUNT(1) FROM MapWeapon WHERE MapID = @MapId AND WeaponID = @WeaponId", connection))
                        {
                            checkCommand.Parameters.AddWithValue("@MapId", newMapId);
                            checkCommand.Parameters.AddWithValue("@WeaponId", weaponId);
                            exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
                        }

                        // If the weapon exists and the map-weapon link does not exist, insert the link into 'MapWeapon'
                        if (weaponId != -1 && !exists)
                        {
                            using (var insertCommand = new SqliteCommand(@"INSERT INTO MapWeapon (MapID, WeaponID) VALUES (@MapId, @WeaponId)", connection))
                            {
                                insertCommand.Parameters.AddWithValue("@MapId", newMapId);
                                insertCommand.Parameters.AddWithValue("@WeaponId", weaponId);
                                await insertCommand.ExecuteNonQueryAsync();
                            }
                        }
                    });
                }
            }


            if (map.Items != null)
            {
                foreach (var itemName in map.Items)
                {
                    DbQueue.Instance.Enqueue(async connection =>
                    {
                        // First, get the ItemID from the 'Item' table
                        int itemId = -1;
                        using (var command = new SqliteCommand("SELECT ItemID FROM Item WHERE Item = @ItemName", connection))
                        {
                            command.Parameters.AddWithValue("@ItemName", itemName);
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    itemId = reader.GetInt32(0); // Assuming 'ItemID' is the first column
                                }
                            }
                        }

                        // Check if the map-item link already exists in 'MapItem'
                        bool exists = false;
                        using (var checkCommand = new SqliteCommand("SELECT COUNT(1) FROM MapItem WHERE MapID = @MapId AND ItemID = @ItemId", connection))
                        {
                            checkCommand.Parameters.AddWithValue("@MapId", newMapId);
                            checkCommand.Parameters.AddWithValue("@ItemId", itemId);
                            exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
                        }

                        // If the item exists and the map-item link does not exist, insert the link into 'MapItem'
                        if (itemId != -1 && !exists)
                        {
                            using (var insertCommand = new SqliteCommand(@"INSERT INTO MapItem (MapID, ItemID) VALUES (@MapId, @ItemId)", connection))
                            {
                                insertCommand.Parameters.AddWithValue("@MapId", newMapId); // Ensure you have the map's ID available
                                insertCommand.Parameters.AddWithValue("@ItemId", itemId);
                                await insertCommand.ExecuteNonQueryAsync();
                            }
                        }
                    });
                }
            }

            if (map.Functions != null)
            {
                foreach (var functionName in map.Functions)
                {
                    DbQueue.Instance.Enqueue(async connection =>
                    {
                        // First, get the FunctionID from the 'Function' table
                        int functionId = -1;
                        using (var command = new SqliteCommand("SELECT FunctionID FROM Function WHERE Function = @FunctionName", connection))
                        {
                            command.Parameters.AddWithValue("@FunctionName", functionName);
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    functionId = reader.GetInt32(0); // Assuming 'FunctionID' is the first column
                                }
                            }
                        }

                        // Check if the map-function link already exists in 'MapFunction'
                        bool exists = false;
                        using (var checkCommand = new SqliteCommand("SELECT COUNT(1) FROM MapFunction WHERE MapID = @MapId AND FunctionID = @FunctionId", connection))
                        {
                            checkCommand.Parameters.AddWithValue("@MapId", newMapId);
                            checkCommand.Parameters.AddWithValue("@FunctionId", functionId);
                            exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
                        }

                        // If the function exists and the map-function link does not exist, insert the link into 'MapFunction'
                        if (functionId != -1 && !exists)
                        {
                            using (var insertCommand = new SqliteCommand(@"INSERT INTO MapFunction (MapID, FunctionID) VALUES (@MapId, @FunctionId)", connection))
                            {
                                insertCommand.Parameters.AddWithValue("@MapId", newMapId);
                                insertCommand.Parameters.AddWithValue("@FunctionId", functionId);
                                await insertCommand.ExecuteNonQueryAsync();
                            }
                        }
                    });
                }
            }






        }



        /// <summary>
        /// /method to add map to db
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>



        public async Task UpdateMap(Map map)
        {
            DbQueue.Instance.Enqueue(async connection =>
            {
                using (var command = new SqliteCommand(@"UPDATE Maps SET 
            Name = @Name, 
            Releasedate = @Releasedate, 
            Author = @Author, 
            Mod = @Mod, 
            Size = @Size, 
            Physics = @Physics, 
            Hits = @Hits, 
            LinkDetailpage = @LinkDetailpage, 
            Style = @Style, 
            LinksOnlineRecordsQ3DFVQ3 = @LinksOnlineRecordsQ3DFVQ3, 
            LinksOnlineRecordsQ3DFCPM = @LinksOnlineRecordsQ3DFCPM, 
            LinksOnlineRecordsRacingVQ3 = @LinksOnlineRecordsRacingVQ3, 
            LinksOnlineRecordsRacingCPM = @LinksOnlineRecordsRacingCPM, 
            LinkDemosVQ3 = @LinkDemosVQ3, 
            LinkDemosCPM = @LinkDemosCPM, 
            DependenciesTextures = @DependenciesTextures
            WHERE Mapname = @Mapname AND Filename = @Filename", connection))
                {
                    command.Parameters.AddWithValue("@Mapname", map.Mapname ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Filename", map.Filename ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Name", map.Name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Releasedate", map.Releasedate ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Author", map.Author ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Mod", map.GameType ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Size", map.Size != -1 ? (object)map.Size : DBNull.Value);
                    command.Parameters.AddWithValue("@Physics", map.Physics != -1 ? (object)map.Physics : DBNull.Value);
                    command.Parameters.AddWithValue("@Hits", map.Hits != -1 ? (object)map.Hits : DBNull.Value);
                    command.Parameters.AddWithValue("@LinkDetailpage", map.LinkDetailpage ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Style", map.Style ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LinksOnlineRecordsQ3DFVQ3", map.LinksOnlineRecordsQ3DFVQ3 ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LinksOnlineRecordsQ3DFCPM", map.LinksOnlineRecordsQ3DFCPM ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LinksOnlineRecordsRacingVQ3", map.LinksOnlineRecordsRacingVQ3 ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LinksOnlineRecordsRacingCPM", map.LinksOnlineRecordsRacingCPM ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LinkDemosVQ3", map.LinkDemosVQ3 ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LinkDemosCPM", map.LinkDemosCPM ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DependenciesTextures", map.Dependencies ?? (object)DBNull.Value);

                    await command.ExecuteNonQueryAsync();
                }
            });
            await Task.CompletedTask;

            MessageHelper.Log($"Updated Base data for {map.Mapname}");

            if (map.Weapons != null)
            {
                //MessageHelper.Log($"Updating {map.Weapons.Count} Weapons for {map.Mapname}");

                foreach (var weaponName in map.Weapons)
                {
                    //MessageHelper.Log($"Updating {weaponName} for {map.Mapname}");
                    DbQueue.Instance.Enqueue(async connection =>
                    {
                        // First, get the WeaponID from the 'Weapons' table
                        int weaponId = -1;
                        using (var command = new SqliteCommand("SELECT WeaponID FROM Weapon WHERE Weapon = @Weapon", connection))
                        {
                            command.Parameters.AddWithValue("@Weapon", weaponName);
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    weaponId = reader.GetInt32(0); // Assuming 'WeaponID' is the first column
                                }
                            }
                        }

                        // Check if the map-weapon link already exists in 'MapWeapon'
                        bool exists = false;
                        using (var checkCommand = new SqliteCommand("SELECT COUNT(1) FROM MapWeapon WHERE MapID = @MapId AND WeaponID = @WeaponId", connection))
                        {
                            checkCommand.Parameters.AddWithValue("@MapId", map.Id);
                            checkCommand.Parameters.AddWithValue("@WeaponId", weaponId);
                            exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
                        }

                        // If the weapon exists and the map-weapon link does not exist, insert the link into 'MapWeapon'
                        if (weaponId != -1 && !exists)
                        {
                            using (var insertCommand = new SqliteCommand(@"INSERT INTO MapWeapon (MapID, WeaponID) VALUES (@MapId, @WeaponId)", connection))
                            {
                                insertCommand.Parameters.AddWithValue("@MapId", map.Id);
                                insertCommand.Parameters.AddWithValue("@WeaponId", weaponId);
                                await insertCommand.ExecuteNonQueryAsync();
                            }
                        }
                    });
                }
            }
            if (map.Items != null)
            {
                //MessageHelper.Log($"Updating {map.Items.Count} Items for {map.Mapname}");

                foreach (var itemName in map.Items)
                {
                    //MessageHelper.Log($"Updating {itemName} for {map.Mapname}");
                    DbQueue.Instance.Enqueue(async connection =>
                    {
                        // First, get the ItemID from the 'Item' table
                        int itemId = -1;
                        using (var command = new SqliteCommand("SELECT ItemID FROM Item WHERE Item = @ItemName", connection))
                        {
                            command.Parameters.AddWithValue("@ItemName", itemName);
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    itemId = reader.GetInt32(0); // Assuming 'ItemID' is the first column
                                }
                            }
                        }

                        // Check if the map-item link already exists in 'MapItem'
                        bool exists = false;
                        using (var checkCommand = new SqliteCommand("SELECT COUNT(1) FROM MapItem WHERE MapID = @MapId AND ItemID = @ItemId", connection))
                        {
                            checkCommand.Parameters.AddWithValue("@MapId", map.Id);
                            checkCommand.Parameters.AddWithValue("@ItemId", itemId);
                            exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
                        }

                        // If the item exists and the map-item link does not exist, insert the link into 'MapItem'
                        if (itemId != -1 && !exists)
                        {
                            using (var insertCommand = new SqliteCommand(@"INSERT INTO MapItem (MapID, ItemID) VALUES (@MapId, @ItemId)", connection))
                            {
                                insertCommand.Parameters.AddWithValue("@MapId", map.Id); // Ensure you have the map's ID available
                                insertCommand.Parameters.AddWithValue("@ItemId", itemId);
                                await insertCommand.ExecuteNonQueryAsync();
                            }
                        }
                    });
                }
            }

            if (map.Functions != null)
            {
                //MessageHelper.Log($"Updating {map.Function.Count} Function for {map.Mapname}");

                foreach (var functionName in map.Functions)
                {
                    //MessageHelper.Log($"Updating {functionName} for {map.Mapname}");
                    DbQueue.Instance.Enqueue(async connection =>
                    {
                        // First, get the FunctionID from the 'Function' table
                        int functionId = -1;
                        using (var command = new SqliteCommand("SELECT FunctionID FROM \"Function\" WHERE \"Function\" = @FunctionName", connection))
                        {
                            command.Parameters.AddWithValue("@FunctionName", functionName);
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    functionId = reader.GetInt32(0); // Assuming 'FunctionID' is the first column
                                }
                            }
                        }

                        // Check if the map-function link already exists in 'MapFunction'
                        bool exists = false;
                        using (var checkCommand = new SqliteCommand("SELECT COUNT(1) FROM MapFunction WHERE MapID = @MapId AND FunctionID = @FunctionId", connection))
                        {
                            checkCommand.Parameters.AddWithValue("@MapId", map.Id);
                            checkCommand.Parameters.AddWithValue("@FunctionId", functionId);
                            exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
                        }

                        // If the function exists and the map-function link does not exist, insert the link into 'MapFunction'
                        if (functionId != -1 && !exists)
                        {
                            using (var insertCommand = new SqliteCommand(@"INSERT INTO MapFunction (MapID, FunctionID) VALUES (@MapId, @FunctionId)", connection))
                            {
                                insertCommand.Parameters.AddWithValue("@MapId", map.Id); // Ensure you have the map's ID available
                                insertCommand.Parameters.AddWithValue("@FunctionId", functionId);
                                try
                                {
                                    await insertCommand.ExecuteNonQueryAsync();
                                }
                                catch (Exception e) { MessageHelper.Log(e.Message); }
                            }
                        }
                    });
                }
            }




        }



        //method to set the parsed flag in Maplist for the given map
        public async Task SetMapParsed(MapData map)
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






    }
}
