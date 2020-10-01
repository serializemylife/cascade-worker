using MySql.Data.MySqlClient;

namespace CascadeWorker.Database
{
    public class DatabaseProvider : IDatabaseProvider
    {
        private readonly string _connectionString;

        public DatabaseProvider(string databaseName, string password, uint port, string host, string username)
        {
            _connectionString = new MySqlConnectionStringBuilder
            {
                Database = databaseName,
                Password = password,
                Port = port,
                Server = host,
                UserID = username,
                SslMode = MySqlSslMode.None,
                DefaultCommandTimeout = 30,
                CharacterSet = "utf8mb4",
                AllowUserVariables = true,
                MinimumPoolSize = 1,
            }.ToString();
        }

        public DatabaseConnection GetConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            var command = connection.CreateCommand();
            
            return new DatabaseConnection(connection, command);
        }

        public bool IsConnected()
        {
            try
            {
                GetConnection();
                return true;
            }
            catch (MySqlException)
            {
                return false;
            }
        }
    }
}