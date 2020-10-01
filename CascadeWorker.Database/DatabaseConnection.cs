using System;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace CascadeWorker.Database
{
    public class DatabaseConnection : IDisposable
    {
        private readonly MySqlConnection _connection;
        private readonly MySqlCommand _command;

        public DatabaseConnection(MySqlConnection connection, MySqlCommand command)
        {
            _connection = connection;
            _command = command;

            _connection.Open();
        }

        public void SetQuery(string commandText)
        {
            _command.Parameters.Clear();
            _command.CommandText = commandText;
        }

        public int ExecuteQuery()
        {
            return _command.ExecuteNonQuery();
        }

        public Task ExecuteQueryAsync()
        {
            return _command.ExecuteNonQueryAsync();
        }

        public DataTable ExecuteTable()
        {
            var dataTable = new DataTable();

            using var adapter = new MySqlDataAdapter(_command);
            adapter.Fill(dataTable);

            return dataTable;
        }

        public DataRow ExecuteRow()
        {
            DataRow dataRow = null;

            var dataSet = new DataSet();

            using (var adapter = new MySqlDataAdapter(_command))
            {
                adapter.Fill(dataSet);
            }

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count == 1)
            {
                dataRow = dataSet.Tables[0].Rows[0];
            }

            return dataRow;
        }


        public object ExecuteScalar()
        {
            return _command.ExecuteScalar();
        }

        public int GetLastId()
        {
            SetQuery("SELECT LAST_INSERT_ID();");
            return int.Parse(ExecuteScalar().ToString());
        }

        public void AddParameter(string name, object value)
        {
            _command.Parameters.AddWithValue(name, value);
        }

        public void Dispose()
        {
            _connection.Close();
            _command.Dispose();
        }
    }
}