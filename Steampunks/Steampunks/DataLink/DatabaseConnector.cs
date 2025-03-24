using System;
using Microsoft.Data.SqlClient;

namespace Steampunks.DataLink
{
    public class DatabaseConnector
    {
        private readonly string connectionString;
        private SqlConnection? connection;

        public DatabaseConnector()
        {
            // Local MSSQL connection string
            connectionString = @"Server=localhost;Database=SteampunksDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public SqlConnection GetConnection()
        {
            if (connection == null)
            {
                connection = new SqlConnection(connectionString);
            }
            return connection;
        }

        public void OpenConnection()
        {
            if (connection?.State != System.Data.ConnectionState.Open)
            {
                connection?.Open();
            }
        }

        public void CloseConnection()
        {
            if (connection?.State != System.Data.ConnectionState.Closed)
            {
                connection?.Close();
            }
        }

        public void ExecuteStoredProcedure(string procedureName, params SqlParameter[] parameters)
        {
            using (var command = new SqlCommand(procedureName, GetConnection()))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                try
                {
                    OpenConnection();
                    command.ExecuteNonQuery();
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    OpenConnection();
                    using (var command = new SqlCommand("SELECT 1", connection))
                    {
                        command.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine("Database query test successful!");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database query test failed: {ex.Message}");
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }
    }
} 