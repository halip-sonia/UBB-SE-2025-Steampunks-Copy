using System;
using Microsoft.Data.SqlClient;
using System.Data;

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

        public int AddGame(string title, float price, string genre, string description, string status = "Available", float? recommendedSpecs = null, float? minimumSpecs = null)
        {
            const string query = @"
                INSERT INTO Games (Title, Price, Genre, Description, Status, RecommendedSpecs, MinimumSpecs)
                VALUES (@Title, @Price, @Genre, @Description, @Status, @RecommendedSpecs, @MinimumSpecs);
                SELECT SCOPE_IDENTITY();";

            try
            {
                using (var command = new SqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Price", price);
                    command.Parameters.AddWithValue("@Genre", genre);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@Status", status);
                    command.Parameters.AddWithValue("@RecommendedSpecs", recommendedSpecs ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MinimumSpecs", minimumSpecs ?? (object)DBNull.Value);

                    OpenConnection();
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        public int AddItem(string itemName, int gameId, float price, string description, bool isListed = false)
        {
            const string query = @"
                INSERT INTO Items (ItemName, CorrespondingGameId, Price, Description, IsListed)
                VALUES (@ItemName, @GameId, @Price, @Description, @IsListed);
                SELECT SCOPE_IDENTITY();";

            try
            {
                using (var command = new SqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@ItemName", itemName);
                    command.Parameters.AddWithValue("@GameId", gameId);
                    command.Parameters.AddWithValue("@Price", price);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@IsListed", isListed);

                    OpenConnection();
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        public void AddGameWithItems(string gameTitle, float gamePrice, string genre, string description, 
            params (string Name, float Price, string Description)[] items)
        {
            try
            {
                OpenConnection();
                using (var transaction = GetConnection().BeginTransaction())
                {
                    try
                    {
                        // Add the game first
                        const string gameQuery = @"
                            INSERT INTO Games (Title, Price, Genre, Description, Status)
                            VALUES (@Title, @Price, @Genre, @Description, 'Available');
                            SELECT SCOPE_IDENTITY();";

                        int gameId;
                        using (var command = new SqlCommand(gameQuery, GetConnection(), transaction))
                        {
                            command.Parameters.AddWithValue("@Title", gameTitle);
                            command.Parameters.AddWithValue("@Price", gamePrice);
                            command.Parameters.AddWithValue("@Genre", genre);
                            command.Parameters.AddWithValue("@Description", description);
                            gameId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // Add each item
                        const string itemQuery = @"
                            INSERT INTO Items (ItemName, CorrespondingGameId, Price, Description, IsListed)
                            VALUES (@ItemName, @GameId, @Price, @Description, 0);";

                        foreach (var item in items)
                        {
                            using (var command = new SqlCommand(itemQuery, GetConnection(), transaction))
                            {
                                command.Parameters.AddWithValue("@ItemName", item.Name);
                                command.Parameters.AddWithValue("@GameId", gameId);
                                command.Parameters.AddWithValue("@Price", item.Price);
                                command.Parameters.AddWithValue("@Description", item.Description);
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            finally
            {
                CloseConnection();
            }
        }
    }
} 