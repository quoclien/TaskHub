using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace TaskHub.Api.IntegrationTests
{
    public static class DatabaseManager
    {
        private static readonly SqliteConnection _connection;

        static DatabaseManager()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        public static DbConnection Connection => _connection;

        public static void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}
