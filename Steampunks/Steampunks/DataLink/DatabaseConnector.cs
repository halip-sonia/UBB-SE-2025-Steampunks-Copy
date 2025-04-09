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

    public class DatabaseConnector: IDatabaseConnector
    {
        private readonly string connectionString;
        private SqlConnection? connection;

        public DatabaseConnector()
        {
            // Local MSSQL connection string
            this.connectionString = Configuration.CONNECTIONSTRINGILINCA;
        }

        public SqlConnection GetConnection()
        {
            if (this.connection == null || this.connection.State == ConnectionState.Closed)
            {
                this.connection = new SqlConnection(this.connectionString);
            }

            return this.connection;
        }

        public SqlConnection GetNewConnection()
        {
            return new SqlConnection(this.connectionString);
        }

        public void OpenConnection()
        {
            if (this.connection?.State != ConnectionState.Open)
            {
                this.connection?.Open();
            }
        }

        public async Task OpenConnectionAsync()
        {
            if (this.connection == null)
            {
                this.connection = new SqlConnection(this.connectionString);
            }

            if (this.connection.State != ConnectionState.Open)
            {
                await this.connection.OpenAsync().ConfigureAwait(false);
            }
        }

        public void CloseConnection()
        {
            if (this.connection?.State != ConnectionState.Closed)
            {
                this.connection?.Close();
            }
        }

        public async Task CloseConnectionAsync()
        {
            if (this.connection?.State != ConnectionState.Closed)
            {
                await Task.Run(() => this.connection.Close());
            }
        }

        public List<Game> GetAllGames()
        {
            var games = new List<Game>();
            using (var command = new SqlCommand("SELECT GameId, Title, Price, Genre, Description, Status FROM Games", this.GetConnection()))
            {
                try
                {
                    this.OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = new Game(
                                reader.GetString(reader.GetOrdinal("Title")),
                                (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                reader.GetString(reader.GetOrdinal("Genre")),
                                reader.GetString(reader.GetOrdinal("Description")));
                            game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));
                            games.Add(game);
                        }
                    }
                }
                finally
                {
                    this.CloseConnection();
                }
            }

            return games;
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

        public void TestDatabaseData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Testing database data...");

                using (var connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    // Test Users table
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM Users", connection))
                    {
                        var userCount = (int)command.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine($"Users in database: {userCount}");
                    }

                    // Test Games table
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM Games", connection))
                    {
                        var gameCount = (int)command.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine($"Games in database: {gameCount}");
                    }

                    // Test Items table
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM Items", connection))
                    {
                        var itemCount = (int)command.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine($"Items in database: {itemCount}");
                    }

                    // Test UserInventory table
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM UserInventory", connection))
                    {
                        var inventoryCount = (int)command.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine($"Items in UserInventory: {inventoryCount}");
                    }

                    // Test specific user's inventory
                    using (var command = new SqlCommand(@"SELECT COUNT(*) FROM UserInventory ui JOIN Items i ON ui.ItemId = i.ItemId JOIN Games g ON ui.GameId = g.GameId WHERE ui.UserId = 1", connection))
                    {
                        var user1InventoryCount = (int)command.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine($"Items in TestUser1's inventory: {user1InventoryCount}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TestDatabaseData: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                throw;
            }
        }

        public void CreateItemTrade(ItemTrade trade)
        {
            const string insertTrade = @"
                INSERT INTO ItemTrades (SourceUserId, DestinationUserId, GameOfTradeId, TradeDate, TradeDescription, TradeStatus, AcceptedBySourceUser, AcceptedByDestinationUser)
                OUTPUT INSERTED.TradeId
                VALUES (@SourceUserId, @DestinationUserId, @GameId, @TradeDate, @TradeDescription, @TradeStatus, 1, 0)";

            const string insertTradeDetails = @"
                INSERT INTO ItemTradeDetails (TradeId, ItemId, IsSourceUserItem)
                VALUES (@TradeId, @ItemId, @IsSourceUserItem)";

            try
            {
                this.OpenConnection();
                using (var transaction = this.GetConnection().BeginTransaction())
                {
                    try
                    {
                        // Insert trade (note that AcceptedBySourceUser is set to 1)
                        int tradeId;
                        using (var command = new SqlCommand(insertTrade, this.GetConnection(), transaction))
                        {
                            command.Parameters.AddWithValue("@SourceUserId", trade.SourceUser.UserId);
                            command.Parameters.AddWithValue("@DestinationUserId", trade.DestinationUser.UserId);
                            command.Parameters.AddWithValue("@GameId", trade.GameOfTrade.GameId);
                            command.Parameters.AddWithValue("@TradeDate", trade.TradeDate);
                            command.Parameters.AddWithValue("@TradeDescription", trade.TradeDescription);
                            command.Parameters.AddWithValue("@TradeStatus", "Pending");

                            tradeId = (int)command.ExecuteScalar();
                        }

                        // Insert source user items
                        foreach (var item in trade.SourceUserItems)
                        {
                            using (var command = new SqlCommand(insertTradeDetails, this.GetConnection(), transaction))
                            {
                                command.Parameters.AddWithValue("@TradeId", tradeId);
                                command.Parameters.AddWithValue("@ItemId", item.ItemId);
                                command.Parameters.AddWithValue("@IsSourceUserItem", true);
                                command.ExecuteNonQuery();
                            }
                        }

                        // Insert destination user items
                        foreach (var item in trade.DestinationUserItems)
                        {
                            using (var command = new SqlCommand(insertTradeDetails, this.GetConnection(), transaction))
                            {
                                command.Parameters.AddWithValue("@TradeId", tradeId);
                                command.Parameters.AddWithValue("@ItemId", item.ItemId);
                                command.Parameters.AddWithValue("@IsSourceUserItem", false);
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        trade.SetTradeId(tradeId);
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
                this.CloseConnection();
            }
        }

        public List<ItemTrade> GetActiveItemTrades(int userId)
        {
            var trades = new List<ItemTrade>();
            const string tradesQuery = @"
                SELECT t.*, 
                       su.UserId as SourceUserId, su.Username as SourceUsername,
                       du.UserId as DestUserId, du.Username as DestUsername,
                       g.GameId, g.Title as GameTitle, g.Price as GamePrice, g.Genre, g.Description as GameDescription
                FROM ItemTrades t
                JOIN Users su ON t.SourceUserId = su.UserId
                JOIN Users du ON t.DestinationUserId = du.UserId
                JOIN Games g ON t.GameOfTradeId = g.GameId
                WHERE (t.SourceUserId = @UserId OR t.DestinationUserId = @UserId)
                AND t.TradeStatus = 'Pending'";

            const string itemsQuery = @"
                SELECT i.*, td.IsSourceUserItem,
                       g.GameId, g.Title as GameTitle, g.Price as GamePrice, g.Genre, g.Description as GameDescription
                FROM ItemTradeDetails td
                JOIN Items i ON td.ItemId = i.ItemId
                JOIN Games g ON i.CorrespondingGameId = g.GameId
                WHERE td.TradeId = @TradeId";

            try
            {
                this.OpenConnection();

                // First, get all trades
                using (var command = new SqlCommand(tradesQuery, this.GetConnection()))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var sourceUser = new User(
                                reader.GetString(reader.GetOrdinal("SourceUsername")));
                            sourceUser.SetUserId(reader.GetInt32(reader.GetOrdinal("SourceUserId")));

                            var destUser = new User(
                                reader.GetString(reader.GetOrdinal("DestUsername")));
                            destUser.SetUserId(reader.GetInt32(reader.GetOrdinal("DestUserId")));

                            var game = new Game(
                                reader.GetString(reader.GetOrdinal("GameTitle")),
                                (float)reader.GetDouble(reader.GetOrdinal("GamePrice")),
                                reader.GetString(reader.GetOrdinal("Genre")),
                                reader.GetString(reader.GetOrdinal("GameDescription")));
                            game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));

                            var trade = new ItemTrade(
                                sourceUser,
                                destUser,
                                game,
                                reader.GetString(reader.GetOrdinal("TradeDescription")));

                            trade.SetTradeId(reader.GetInt32(reader.GetOrdinal("TradeId")));

                            // Set the trade status from the database
                            string tradeStatus = reader.GetString(reader.GetOrdinal("TradeStatus"));
                            if (tradeStatus == "Completed")
                            {
                                trade.Complete();
                            }
                            else if (tradeStatus == "Declined")
                            {
                                trade.Decline();
                            }

                            trades.Add(trade);
                        }
                    }
                }

                // Then, for each trade, get its items
                foreach (var trade in trades)
                {
                    using (var command = new SqlCommand(itemsQuery, this.GetConnection()))
                    {
                        command.Parameters.AddWithValue("@TradeId", trade.TradeId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var game = new Game(
                                    reader.GetString(reader.GetOrdinal("GameTitle")),
                                    (float)reader.GetDouble(reader.GetOrdinal("GamePrice")),
                                    reader.GetString(reader.GetOrdinal("Genre")),
                                    reader.GetString(reader.GetOrdinal("GameDescription")));
                                game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));

                                var item = new Item(
                                    reader.GetString(reader.GetOrdinal("ItemName")),
                                    game,
                                    (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                    reader.GetString(reader.GetOrdinal("Description")));
                                item.SetItemId(reader.GetInt32(reader.GetOrdinal("ItemId")));
                                item.SetIsListed(reader.GetBoolean(reader.GetOrdinal("IsListed")));

                                bool isSourceUserItem = reader.GetBoolean(reader.GetOrdinal("IsSourceUserItem"));
                                if (isSourceUserItem)
                                {
                                    trade.AddSourceUserItem(item);
                                }
                                else
                                {
                                    trade.AddDestinationUserItem(item);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                this.CloseConnection();
            }

            return trades;
        }

        public List<ItemTrade> GetItemTradeHistory(int userId)
        {
            var trades = new List<ItemTrade>();
            const string tradesQuery = @"
                SELECT t.*, 
                       su.UserId as SourceUserId, su.Username as SourceUsername,
                       du.UserId as DestUserId, du.Username as DestUsername,
                       g.GameId, g.Title as GameTitle, g.Price as GamePrice, g.Genre, g.Description as GameDescription
                FROM ItemTrades t
                JOIN Users su ON t.SourceUserId = su.UserId
                JOIN Users du ON t.DestinationUserId = du.UserId
                JOIN Games g ON t.GameOfTradeId = g.GameId
                WHERE (t.SourceUserId = @UserId OR t.DestinationUserId = @UserId)
                AND t.TradeStatus IN ('Completed', 'Declined')";

            const string itemsQuery = @"
                SELECT i.*, td.IsSourceUserItem,
                       g.GameId, g.Title as GameTitle, g.Price as GamePrice, g.Genre, g.Description as GameDescription
                FROM ItemTradeDetails td
                JOIN Items i ON td.ItemId = i.ItemId
                JOIN Games g ON i.CorrespondingGameId = g.GameId
                WHERE td.TradeId = @TradeId";

            try
            {
                this.OpenConnection();

                // First, get all trades
                using (var command = new SqlCommand(tradesQuery, this.GetConnection()))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var sourceUser = new User(
                                reader.GetString(reader.GetOrdinal("SourceUsername")));
                            sourceUser.SetUserId(reader.GetInt32(reader.GetOrdinal("SourceUserId")));

                            var destUser = new User(
                                reader.GetString(reader.GetOrdinal("DestUsername")));
                            destUser.SetUserId(reader.GetInt32(reader.GetOrdinal("DestUserId")));

                            var game = new Game(
                                reader.GetString(reader.GetOrdinal("GameTitle")),
                                (float)reader.GetDouble(reader.GetOrdinal("GamePrice")),
                                reader.GetString(reader.GetOrdinal("Genre")),
                                reader.GetString(reader.GetOrdinal("GameDescription")));
                            game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));

                            var trade = new ItemTrade(
                                sourceUser,
                                destUser,
                                game,
                                reader.GetString(reader.GetOrdinal("TradeDescription")));
                            trade.SetTradeId(reader.GetInt32(reader.GetOrdinal("TradeId")));

                            // Set the trade status from the database
                            string tradeStatus = reader.GetString(reader.GetOrdinal("TradeStatus"));
                            if (tradeStatus == "Completed")
                            {
                                trade.Complete();
                            }
                            else if (tradeStatus == "Declined")
                            {
                                trade.Decline();
                            }

                            trades.Add(trade);
                        }
                    }
                }

                // Then, for each trade, get its items
                foreach (var trade in trades)
                {
                    using (var command = new SqlCommand(itemsQuery, this.GetConnection()))
                    {
                        command.Parameters.AddWithValue("@TradeId", trade.TradeId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var game = new Game(
                                    reader.GetString(reader.GetOrdinal("GameTitle")),
                                    (float)reader.GetDouble(reader.GetOrdinal("GamePrice")),
                                    reader.GetString(reader.GetOrdinal("Genre")),
                                    reader.GetString(reader.GetOrdinal("GameDescription")));

                                game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));

                                var item = new Item(
                                    reader.GetString(reader.GetOrdinal("ItemName")),
                                    game,
                                    (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                    reader.GetString(reader.GetOrdinal("Description")));

                                item.SetItemId(reader.GetInt32(reader.GetOrdinal("ItemId")));
                                item.SetIsListed(reader.GetBoolean(reader.GetOrdinal("IsListed")));

                                bool isSourceUserItem = reader.GetBoolean(reader.GetOrdinal("IsSourceUserItem"));
                                if (isSourceUserItem)
                                {
                                    trade.AddSourceUserItem(item);
                                }
                                else
                                {
                                    trade.AddDestinationUserItem(item);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                this.CloseConnection();
            }

            return trades;
        }

        public List<Item> GetUserItems(int userId)
        {
            var items = new List<Item>();
            const string query = @"
                SELECT 
                    i.ItemId,
                    i.ItemName,
                    i.Price,
                    i.Description,
                    i.IsListed,
                    g.GameId,
                    g.Title as GameTitle,
                    g.Price as GamePrice,
                    g.Genre,
                    g.Description as GameDescription,
                    g.Status as GameStatus
                FROM UserInventory ui
                JOIN Items i ON ui.ItemId = i.ItemId
                JOIN Games g ON ui.GameId = g.GameId
                WHERE ui.UserId = @UserId";

            try
            {
                using (var command = new SqlCommand(query, this.GetConnection()))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    this.OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = new Game(
                                reader.GetString(reader.GetOrdinal("GameTitle")),
                                (float)reader.GetDouble(reader.GetOrdinal("GamePrice")),
                                reader.GetString(reader.GetOrdinal("Genre")),
                                reader.GetString(reader.GetOrdinal("GameDescription")));
                            game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));
                            game.SetStatus(reader.GetString(reader.GetOrdinal("GameStatus")));

                            var item = new Item(
                                reader.GetString(reader.GetOrdinal("ItemName")),
                                game,
                                (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                reader.GetString(reader.GetOrdinal("Description")));
                            item.SetItemId(reader.GetInt32(reader.GetOrdinal("ItemId")));
                            item.SetIsListed(reader.GetBoolean(reader.GetOrdinal("IsListed")));
                            items.Add(item);
                        }
                    }
                }
            }
            finally
            {
                this.CloseConnection();
            }

            return items;
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

        public void CheckDatabaseState()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("\n=== Database State Check ===");
                using (var connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    // Check Users
                    using (var command = new SqlCommand("SELECT UserId, Username FROM Users", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            System.Diagnostics.Debug.WriteLine("\nUsers:");
                            while (reader.Read())
                            {
                                System.Diagnostics.Debug.WriteLine($"User {reader.GetInt32(0)}: {reader.GetString(1)}");
                            }
                        }
                    }

                    // Check Games
                    using (var command = new SqlCommand("SELECT GameId, Title FROM Games", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            System.Diagnostics.Debug.WriteLine("\nGames:");
                            while (reader.Read())
                            {
                                System.Diagnostics.Debug.WriteLine($"Game {reader.GetInt32(0)}: {reader.GetString(1)}");
                            }
                        }
                    }

                    // Check Items
                    using (var command = new SqlCommand("SELECT ItemId, ItemName, CorrespondingGameId FROM Items", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            System.Diagnostics.Debug.WriteLine("\nItems:");
                            while (reader.Read())
                            {
                                System.Diagnostics.Debug.WriteLine($"Item {reader.GetInt32(0)}: {reader.GetString(1)} (GameId: {reader.GetInt32(2)})");
                            }
                        }
                    }

                    // Check UserInventory
                    using (var command = new SqlCommand(@"SELECT ui.UserId, ui.GameId, ui.ItemId, u.Username, g.Title as GameTitle, i.ItemName  FROM UserInventory ui JOIN Users u ON ui.UserId = u.UserId JOIN Games g ON ui.GameId = g.GameId JOIN Items i ON ui.ItemId = i.ItemId", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            System.Diagnostics.Debug.WriteLine("\nUserInventory:");
                            while (reader.Read())
                            {
                                System.Diagnostics.Debug.WriteLine($"User {reader.GetString(3)} has {reader.GetString(5)} from {reader.GetString(4)}");
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("\n=== End Database State Check ===\n");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking database state: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                throw;
            }
        }

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

        internal string GetItemImagePath(Item item)
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

        private void InsertTestGames()
        {
            var testGames = new List<(string title, float price, string genre, string description)>
            {
                ("Counter-Strike 2", 0.0f, "FPS", "The next evolution of Counter-Strike"),
                ("Dota 2", 0.0f, "MOBA", "A complex game of strategy and teamwork"),
                ("Red Dead Redemption 2", 59.99f, "Action Adventure", "Epic tale of life in America's unforgiving heartland"),
                ("The Witcher 3", 39.99f, "RPG", "An epic role-playing game set in a vast open world"),
                ("Cyberpunk 2077", 59.99f, "RPG", "An open-world action-adventure story set in Night City"),
            };

            using (var command = new SqlCommand(@" INSERT INTO Games (Title, Price, Genre, Description, Status) VALUES (@Title, @Price, @Genre, @Description, 'Available')", this.GetConnection()))
            {
                try
                {
                    this.OpenConnection();
                    foreach (var game in testGames)
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Title", game.title);
                        command.Parameters.AddWithValue("@Price", game.price);
                        command.Parameters.AddWithValue("@Genre", game.genre);
                        command.Parameters.AddWithValue("@Description", game.description);
                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    this.CloseConnection();
                }
            }
        }
    }
}