using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    public class DbQueue
    {
        //get appdata
        private static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string AppDataFolder = System.IO.Path.Combine(AppDataPath, "DeFRaG_Helper");
        private static readonly string DbPath = System.IO.Path.Combine(AppDataFolder, "MapData.db");
        private static readonly Lazy<DbQueue> _instance = new Lazy<DbQueue>(() => new DbQueue($"Data Source={DbPath};"));
        public static DbQueue Instance => _instance.Value;

        private readonly string _connectionString;
        private readonly ConcurrentQueue<Func<SqliteConnection, Task>> _operations = new ConcurrentQueue<Func<SqliteConnection, Task>>();
        private TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private bool _isProcessing = false;

        private DbQueue(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Enqueue(Func<SqliteConnection, Task> operation, [CallerMemberName] string callerMemberName = "")
        {
            _operations.Enqueue(operation);
            lock (_operations) // Use the operations queue itself as a simple lock
            {
                if (!_isProcessing)
                {
                    _isProcessing = true;
                    _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously); // Reset here
                    Task.Run(() => ProcessQueue(callerMemberName)); // Pass the caller name to the processing method
                }
            }
        }

        private async Task ProcessQueue(string callerMemberName)
        {
            while (_operations.TryDequeue(out var operation))
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        await operation(connection);
                    }
                    //MessageHelper.Log($"DbQueue operation completed by {callerMemberName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DbQueue operation failed: {ex.Message} - Called by {callerMemberName}");
                    MessageHelper.Log($"DbQueue operation failed: {ex.Message}, {ex.StackTrace}- Called by {callerMemberName}");
                    _tcs.SetException(ex); // Consider setting the exception to signal failure
                    return; // Exit on failure
                }
            }

            lock (_operations)
            {
                if (!_operations.IsEmpty)
                {
                    // If new operations were enqueued during processing, continue processing without setting _isProcessing to false.
                    MessageHelper.Log("DbQueue operations enqueued during processing");
                    return;
                }
                _isProcessing = false;
            }
            _tcs.SetResult(true); // Signal completion
        }



        public Task WhenAllCompleted() => _tcs.Task;
    }
}
