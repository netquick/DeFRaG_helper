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
        private bool _isProcessing = false;

        public DbQueue(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Enqueue(Func<SqliteConnection, Task> operation)
        {
            _operations.Enqueue(operation);
            if (!_isProcessing)
            {
                _isProcessing = true;
                Task.Run(() => ProcessQueue());
            }
        }

        private async Task ProcessQueue()
        {
            while (_operations.TryDequeue(out var operation))
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await operation(connection);
                }
            }
            _isProcessing = false;
        }
    }
}
