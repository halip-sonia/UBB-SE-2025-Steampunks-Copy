// <copyright file="DatabaseConnector.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.DataLink
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Steampunks.Domain.Entities;
    using Steampunks.Utils;

    /// <summary>
    /// Provides utility methods for managing SQL Server database connections and performing basic operations.
    /// Responsible for opening, closing, and testing connections, as well as executing specific insert logic such as adding games with items.
    /// </summary>
    public class DatabaseConnector: IDatabaseConnector
    {
        private readonly string connectionString;
        private SqlConnection? connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnector"/> class using the connection string defined in the configuration.
        /// </summary>
        public DatabaseConnector()
        {
            // Local MSSQL connection string
            this.connectionString = Configuration.CONNECTIONSTRINGDARIUS;
        }

        /// <summary>
        /// Gets the current SQL connection if open or creates and returns a new one if it's null or closed.
        /// </summary>
        /// <returns>The current or newly initialized <see cref="SqlConnection"/>.</returns>
        public SqlConnection GetConnection()
        {
            if (this.connection == null || this.connection.State == ConnectionState.Closed)
            {
                this.connection = new SqlConnection(this.connectionString);
            }

            return this.connection;
        }

        /// <summary>
        /// Always returns a new instance of <see cref="SqlConnection"/>.
        /// </summary>
        /// <returns>A new <see cref="SqlConnection"/> object.</returns>
        public SqlConnection GetNewConnection()
        {
            return new SqlConnection(this.connectionString);
        }

        /// <summary>
        /// Opens the connection if it is not already open.
        /// </summary>
        public void OpenConnection()
        {
            if (this.connection?.State != ConnectionState.Open)
            {
                this.connection?.Open();
            }
        }

        /// <summary>
        /// Asynchronously opens the connection if it is not already open.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OpenConnectionAsync()
        {
            if (this.connection == null)
            {
                this.connection = new SqlConnection(this.connectionString);
            }

            if (this.connection.State != ConnectionState.Open)
            {
                await this.connection.OpenAsync();
            }
        }

        /// <summary>
        /// Closes the connection if it is not already closed.
        /// </summary>
        public void CloseConnection()
        {
            if (this.connection?.State != ConnectionState.Closed)
            {
                this.connection?.Close();
            }

            this.connection = null;
        }

        /// <summary>
        /// Asynchronously closes the connection if it is not already closed.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CloseConnectionAsync()
        {
            if (this.connection?.State != ConnectionState.Closed)
            {
}
                await Task.Run(() => this.connection?.Close());
            }
        }

        /// <summary>
        /// Tests the connection to the database by attempting to open a connection and executing a simple query.
        /// Logs the name of the connected database if successful.
        /// </summary>
        /// <returns>True if the connection is successful; otherwise, false.</returns>
        public bool TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT DB_NAME()", connection))
                    {
                        var dbName = command.ExecuteScalar().ToString();
                        System.Diagnostics.Debug.WriteLine($"Connected to database: {dbName}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database connection test failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Adds a new game along with its associated items to the database within a single transaction.
        /// If any operation fails, the entire transaction is rolled back.
        /// </summary>
        /// <param name="gameTitle">The title of the game.</param>
        /// <param name="gamePrice">The price of the game.</param>
        /// <param name="genre">The genre of the game.</param>
        /// <param name="description">A description of the game.</param>
        /// <param name="items">An array of items associated with the game, each with a name, price, and description.</param>
        /// <exception cref="Exception">Throws any exception encountered during the database operation.</exception>
        public void AddGameWithItems(string gameTitle, float gamePrice, string genre, string description, params (string Name, float Price, string Description)[] items)
        {
            try
            {
                this.OpenConnection();
                using (var transaction = this.GetConnection().BeginTransaction())
                {
                    try
                    {
                        // Add the game first
                        const string gameQuery = @"
                            INSERT INTO Games (Title, Price, Genre, Description, Status)
                            VALUES (@Title, @Price, @Genre, @Description, 'Available');
                            SELECT SCOPE_IDENTITY();";

                        int gameId;
                        using (var command = new SqlCommand(gameQuery, this.GetConnection(), transaction))
                        {
                            command.Parameters.AddWithValue("@Title", gameTitle);
                            command.Parameters.AddWithValue("@Price", gamePrice);
                            command.Parameters.AddWithValue("@Genre", genre);
                            command.Parameters.AddWithValue("@Description", description);
                            gameId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // Create the Game object
                        var game = new Game(gameTitle, gamePrice, genre, description);
                        game.SetGameId(gameId);

                        // Add each item
                        const string itemQuery = @"
                            INSERT INTO Items (ItemName, GameId, Price, Description, IsListed)
                            VALUES (@ItemName, @GameId, @Price, @Description, 0);";

                        foreach (var item in items)
                        {
                            using (var command = new SqlCommand(itemQuery, this.GetConnection(), transaction))
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
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Error in AddGameWithItems: {ex.Message}");
                        throw;
                    }
                }
            }
            finally
            {
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Disposes of the SqlConnection by closing and nullifying it.
        /// </summary>
        public void DisposeConnection()
        {
            if (this.connection != null)
            {
                if (this.connection.State != ConnectionState.Closed)
                {
                    this.connection.Close();
                }

                this.connection.Dispose();
                this.connection = null;
            }
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (var command = new SqlCommand("SELECT UserId, Username FROM Users", this.GetConnection()))
            {
                try
                {
                    this.OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new User(reader.GetString(reader.GetOrdinal("Username")));
                            user.SetUserId(reader.GetInt32(reader.GetOrdinal("UserId")));
                            users.Add(user);
                        }

                        if (users.Count == 0)
                        {
                            // If no users exist, create test users
                            this.CloseConnection();
                            this.InsertTestUsers();
                            return this.GetAllUsers(); // Recursive call to get the newly inserted users
                        }
                    }
                }
                finally
                {
                    this.CloseConnection();
                }
            }

            return users;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            using (var command = new SqlCommand("SELECT UserId, Username FROM Users", this.GetConnection()))
            {
                try
                {
                    await this.OpenConnectionAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var user = new User(reader.GetString(reader.GetOrdinal("Username")));
                            user.SetUserId(reader.GetInt32(reader.GetOrdinal("UserId")));
                            users.Add(user);
                        }

                        if (users.Count == 0)
                        {
                            // If no users exist, create test users
                            await this.CloseConnectionAsync();
                            this.InsertTestUsers();
                            return await this.GetAllUsersAsync(); // Recursive call to get the newly inserted users
                        }
                    }
                }
                finally
                {
                    await this.CloseConnectionAsync();
                }
            }

            return users;
        }

        public User? GetCurrentUser()
        {
            using (var command = new SqlCommand("SELECT TOP 1 UserId, Username FROM Users", this.GetConnection()))
            {
                try
                {
                    this.OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var user = new User(reader.GetString(reader.GetOrdinal("Username")));
                            user.SetUserId(reader.GetInt32(reader.GetOrdinal("UserId")));
                            return user;
                        }

                        return null;
                    }
                }
                finally
                {
                    this.CloseConnection();
                }
            }
        }

        public void InsertTestUsers()
        {
            var testUsers = new List<string>
            {
                "TestUser1",
                "TestUser2",
                "TestUser3",
            };

            using (var command = new SqlCommand(@" INSERT INTO Users (Username, WalletBalance, PointBalance, IsDeveloper) VALUES (@Username, 1000, 100, 0)", this.GetConnection()))
            {
                try
                {
                    this.OpenConnection();
                    foreach (var username in testUsers)
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Username", username);
                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    this.CloseConnection();
                }
            }
        }


        public string GetItemImagePath(Item item)
        {
            try
            {
                // Get the game folder name based on the game title
                string gameFolder = item.Game.Title.ToLower() switch
                {
                    "counter-strike 2" => "cs2",
                    "dota 2" => "dota2",
                    "team fortress 2" => "tf2",
                    _ => item.Game.Title.ToLower().Replace(" ", string.Empty).Replace(":", string.Empty)
                };

                // Return a path to the image based on the ItemId
                var path = $"ms-appx:///Assets/img/games/{gameFolder}/{item.ItemId}.png";
                System.Diagnostics.Debug.WriteLine($"Generated image path for item {item.ItemId} ({item.ItemName}) from {item.Game.Title}: {path}");
                return path;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetItemImagePath: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return "ms-appx:///Assets/img/games/default-item.png";
            }
        }
    }
}
