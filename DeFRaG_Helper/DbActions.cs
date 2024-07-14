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

        public static DbQueue dbQueue = new DbQueue(connectionString);
        public static DbQueue DbQueueInstance
        {
            get
            {
                lock (_lock)
                {
                    if (dbQueue == null)
                    {
                        if (!Directory.Exists(AppDataFolder))
                        {
                            Directory.CreateDirectory(AppDataFolder);
                        }
                        // Form the connection string explicitly
                        var connectionString = $"Data Source={DbPath};";
                        dbQueue = new DbQueue(connectionString);
                    }
                    return dbQueue;
                }
            }
        }
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
            DbQueueInstance.Enqueue(async connection =>
            {
                using (var command = new SqliteCommand(@"INSERT INTO Maps 
        (Name, Mapname, Filename, Releasedate, Author, Mod, Size, Physics, Hits, LinkDetailpage, Style, LinksOnlineRecordsQ3DFVQ3, LinksOnlineRecordsQ3DFCPM, LinksOnlineRecordsRacingVQ3, LinksOnlineRecordsRacingCPM, LinkDemosVQ3, LinkDemosCPM, DependenciesTextures) 
        VALUES 
        (@Name, @Mapname, @Filename, @Releasedate, @Author, @Mod, @Size, @Physics, @Hits, @LinkDetailpage, @Style, @LinksOnlineRecordsQ3DFVQ3, @LinksOnlineRecordsQ3DFCPM, @LinksOnlineRecordsRacingVQ3, @LinksOnlineRecordsRacingCPM, @LinkDemosVQ3, @LinkDemosCPM, @DependenciesTextures  )", connection))
                {
                    command.Parameters.AddWithValue("@Name", map.Name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Mapname", map.Mapname ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Filename", map.Filename ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Releasedate", map.Releasedate ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Author", map.Author ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Mod", map.GameType ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Size", map.Size != -1 ? (object)map.Size : DBNull.Value);
                    command.Parameters.AddWithValue("@Physics", map.Physics != -1 ? (object)map.Physics : DBNull.Value);
                    command.Parameters.AddWithValue("@Hits", map.Hits != -1 ? (object)map.Size : DBNull.Value);
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
        }



        /// <summary>
        /// /method to add map to db
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>



        public async Task UpdateMap(Map map)
        {
            DbQueueInstance.Enqueue(async connection =>
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
        }



        //method to set the parsed flag in Maplist for the given map
        public async Task SetMapParsed(MapData map)
        {
            DbQueueInstance.Enqueue(async connection =>
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
