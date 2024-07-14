using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    public class DbQueue
    {
        private readonly string _connectionString;
        private readonly ConcurrentQueue<Func<SqliteConnection, Task>> _operations = new ConcurrentQueue<Func<SqliteConnection, Task>>();
        private TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        private bool _isProcessing = false;

        public DbQueue(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Enqueue(Func<SqliteConnection, Task> operation)
        {
            _operations.Enqueue(operation);
            lock (_operations) // Use the operations queue itself as a simple lock
            {
                if (!_isProcessing)
                {
                    _isProcessing = true;
                    _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously); // Reset here
                    Task.Run(() => ProcessQueue());
                }
            }
        }

        private async Task ProcessQueue()
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DbQueue operation failed: {ex.Message}"); // Example logging
                    _tcs.SetException(ex); // Consider setting the exception to signal failure
                    return; // Exit on failure
                }
            }

            lock (_operations)
            {
                if (!_operations.IsEmpty)
                {
                    // If new operations were enqueued during processing, continue processing without setting _isProcessing to false.
                    return;
                }
                _isProcessing = false;
            }
            _tcs.SetResult(true); // Signal completion
        }



        public Task WhenAllCompleted() => _tcs.Task;
    }
}
