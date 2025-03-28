using System;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using Steampunks.Domain.Entities;

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
            if (connection == null || connection.State == ConnectionState.Closed)
            {
                connection = new SqlConnection(connectionString);
            }
            return connection;
        }

        public void OpenConnection()
        {
            if (connection?.State != ConnectionState.Open)
            {
                connection?.Open();
            }
        }

        public void CloseConnection()
        {
            if (connection?.State != ConnectionState.Closed)
            {
                connection?.Close();
            }
        }

        public void DisposeConnection()
        {
            if (connection != null)
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
                connection.Dispose();
                connection = null;
            }
        }

        public List<Game> GetAllGames()
        {
            var games = new List<Game>();
            using (var command = new SqlCommand("SELECT GameId, Title, Price, Genre, Description, Status FROM Games", GetConnection()))
            {
                try
                {
                    OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = new Game(
                                reader.GetString(reader.GetOrdinal("Title")),
                                (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                reader.GetString(reader.GetOrdinal("Genre")),
                                reader.GetString(reader.GetOrdinal("Description"))
                            );
                            game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));
                            games.Add(game);
                        }
                    }
                }
                finally
                {
                    CloseConnection();
                }
            }
            return games;
        }

        private void InsertTestGames()
        {
            var testGames = new List<(string title, float price, string genre, string description)>
            {
                ("Counter-Strike 2", 0.0f, "FPS", "The next evolution of Counter-Strike"),
                ("Dota 2", 0.0f, "MOBA", "A complex game of strategy and teamwork"),
                ("Red Dead Redemption 2", 59.99f, "Action Adventure", "Epic tale of life in America's unforgiving heartland"),
                ("The Witcher 3", 39.99f, "RPG", "An epic role-playing game set in a vast open world"),
                ("Cyberpunk 2077", 59.99f, "RPG", "An open-world action-adventure story set in Night City")
            };

            using (var command = new SqlCommand(@"
                INSERT INTO Games (Title, Price, Genre, Description, Status)
                VALUES (@Title, @Price, @Genre, @Description, 'Available')", GetConnection()))
            {
                try
                {
                    OpenConnection();
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
                    CloseConnection();
                }
            }
        }

        public User GetCurrentUser()
        {
            using (var command = new SqlCommand("SELECT TOP 1 UserId, Username FROM Users", GetConnection()))
            {
                try
                {
                    OpenConnection();
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
                    CloseConnection();
                }
            }
        }

        private void InsertTestUsers()
        {
            var testUsers = new List<string>
            {
                "TestUser1",
                "TestUser2",
                "TestUser3"
            };

            using (var command = new SqlCommand(@"
                INSERT INTO Users (Username, WalletBalance, PointBalance, IsDeveloper)
                VALUES (@Username, 1000, 100, 0)", GetConnection()))
            {
                try
                {
                    OpenConnection();
                    foreach (var username in testUsers)
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Username", username);
                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        public User? GetUserByUsername(string username)
        {
            using (var command = new SqlCommand("SELECT UserId, Username FROM Users WHERE Username = @Username", GetConnection()))
            {
                command.Parameters.AddWithValue("@Username", username);
                try
                {
                    OpenConnection();
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
                    CloseConnection();
                }
            }
        }

        public void CreateGameTrade(GameTrade trade)
        {
            using (var command = new SqlCommand(@"
                INSERT INTO GameTrades (SourceUserId, DestinationUserId, GameId, TradeDescription)
                VALUES (@SourceUserId, @DestinationUserId, @GameId, @TradeDescription);
                SELECT SCOPE_IDENTITY();", GetConnection()))
            {
                command.Parameters.AddWithValue("@SourceUserId", trade.GetSourceUser().UserId);
                command.Parameters.AddWithValue("@DestinationUserId", trade.GetDestinationUser().UserId);
                command.Parameters.AddWithValue("@GameId", trade.GetTradeGame().GameId);
                command.Parameters.AddWithValue("@TradeDescription", trade.TradeDescription);

                try
                {
                    OpenConnection();
                    var tradeId = Convert.ToInt32(command.ExecuteScalar());
                    trade.SetTradeId(tradeId);
                }
                finally
                {
                    CloseConnection();
                }
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
                using (var connection = new SqlConnection(connectionString))
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
        public List<GameTrade> GetActiveGameTrades(int userId)
        {
            var trades = new List<GameTrade>();

            using (var command = new SqlCommand(@"
                SELECT 
                    t.TradeId, t.TradeDate, t.TradeDescription, t.TradeStatus,
                    t.AcceptedBySourceUser, t.AcceptedByDestinationUser,
                    su.UserId as SourceUserId, su.Username as SourceUsername,
                    du.UserId as DestUserId, du.Username as DestUsername,
                    g.GameId, g.Title, g.Price, g.Genre, g.Description
                FROM GameTrades t
                JOIN Users su ON t.SourceUserId = su.UserId
                JOIN Users du ON t.DestinationUserId = du.UserId
                JOIN Games g ON t.GameId = g.GameId
                WHERE (t.SourceUserId = @UserId OR t.DestinationUserId = @UserId)
                AND t.TradeStatus = 'Pending'
                ORDER BY t.TradeDate DESC", GetConnection()))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                try
                {
                    OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var sourceUser = new User(
                                reader.GetString(reader.GetOrdinal("SourceUsername"))
                            );
                            sourceUser.SetUserId(reader.GetInt32(reader.GetOrdinal("SourceUserId")));

                            var destUser = new User(
                                reader.GetString(reader.GetOrdinal("DestUsername"))
                            );
                            destUser.SetUserId(reader.GetInt32(reader.GetOrdinal("DestUserId")));

                            var game = new Game(
                                reader.GetString(reader.GetOrdinal("Title")),
                                (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                reader.GetString(reader.GetOrdinal("Genre")),
                                reader.GetString(reader.GetOrdinal("Description"))
                            );
                            game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));

                            var trade = new GameTrade(sourceUser, destUser, game, reader.GetString(reader.GetOrdinal("TradeDescription")));
                            trade.SetTradeId(reader.GetInt32(reader.GetOrdinal("TradeId")));
                            trade.SetTradeStatus(reader.GetString(reader.GetOrdinal("TradeStatus")));

                            trades.Add(trade);
                        }
                    }
                }
                finally
                {
                    CloseConnection();
                }
            }
            return trades;
        }

        public List<GameTrade> GetTradeHistory(int userId)
        {
            var trades = new List<GameTrade>();

            using (var command = new SqlCommand(@"
                SELECT 
                    t.TradeId, t.TradeDate, t.TradeDescription, t.TradeStatus,
                    t.AcceptedBySourceUser, t.AcceptedByDestinationUser,
                    su.UserId as SourceUserId, su.Username as SourceUsername,
                    du.UserId as DestUserId, du.Username as DestUsername,
                    g.GameId, g.Title, g.Price, g.Genre, g.Description
                FROM GameTrades t
                JOIN Users su ON t.SourceUserId = su.UserId
                JOIN Users du ON t.DestinationUserId = du.UserId
                JOIN Games g ON t.GameId = g.GameId
                WHERE (t.SourceUserId = @UserId OR t.DestinationUserId = @UserId)
                AND t.TradeStatus IN ('Completed', 'Declined')
                ORDER BY t.TradeDate DESC", GetConnection()))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                try
                {
                    OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var sourceUser = new User(
                                reader.GetString(reader.GetOrdinal("SourceUsername"))
                            );
                            sourceUser.SetUserId(reader.GetInt32(reader.GetOrdinal("SourceUserId")));

                            var destUser = new User(
                                reader.GetString(reader.GetOrdinal("DestUsername"))
                            );
                            destUser.SetUserId(reader.GetInt32(reader.GetOrdinal("DestUserId")));

                            var game = new Game(
                                reader.GetString(reader.GetOrdinal("Title")),
                                (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                reader.GetString(reader.GetOrdinal("Genre")),
                                reader.GetString(reader.GetOrdinal("Description"))
                            );
                            game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));

                            var trade = new GameTrade(sourceUser, destUser, game, reader.GetString(reader.GetOrdinal("TradeDescription")));
                            trade.SetTradeId(reader.GetInt32(reader.GetOrdinal("TradeId")));
                            trade.SetTradeStatus(reader.GetString(reader.GetOrdinal("TradeStatus")));

                            trades.Add(trade);
                        }
                    }
                }
                finally
                {
                    CloseConnection();
                }
            }
            return trades;
        }

        public List<GameTrade> GetActiveGameTrades()
        {
            var currentUser = GetCurrentUser();
            return GetActiveGameTrades(currentUser.UserId);
        }

        public List<GameTrade> GetTradeHistory()
        {
            var currentUser = GetCurrentUser();
            return GetTradeHistory(currentUser.UserId);
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (var command = new SqlCommand("SELECT UserId, Username FROM Users", GetConnection()))
            {
                try
                {
                    OpenConnection();
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
                            CloseConnection();
                            InsertTestUsers();
                            return GetAllUsers(); // Recursive call to get the newly inserted users
                        }
                    }
                }
                finally
                {
                    CloseConnection();
                }
            }
            return users;
        }

        public void AcceptTrade(int tradeId)
        {
            using (var command = new SqlCommand(@"
                UPDATE GameTrades 
                SET TradeStatus = 'Completed'
                WHERE TradeId = @TradeId", GetConnection()))
            {
                command.Parameters.AddWithValue("@TradeId", tradeId);

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

        public void DeclineTrade(int tradeId)
        {
            using (var command = new SqlCommand(@"
                UPDATE GameTrades 
                SET TradeStatus = 'Declined'
                WHERE TradeId = @TradeId", GetConnection()))
            {
                command.Parameters.AddWithValue("@TradeId", tradeId);

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
                CloseConnection();
            }
        }

        public List<Item> GetAllListings()
        {
            var listings = new List<Item>();
            const string query = @"
                SELECT 
                    i.ItemId,
                    i.ItemName,
                    i.Price,
                    i.Description,
                    i.IsListed,
                    g.GameId,
                    g.Title as GameTitle,
                    g.Genre,
                    g.Description as GameDescription,
                    g.Price as GamePrice,
                    g.Status as GameStatus
                FROM Items i
                JOIN Games g ON i.CorrespondingGameId = g.GameId
                WHERE i.IsListed = 1
                ORDER BY i.Price DESC";

            try
            {
                using (var command = new SqlCommand(query, GetConnection()))
                {
                    OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = new Game(
                                reader.GetString(reader.GetOrdinal("GameTitle")),
                                (float)reader.GetDouble(reader.GetOrdinal("GamePrice")),
                                reader.GetString(reader.GetOrdinal("Genre")),
                                reader.GetString(reader.GetOrdinal("GameDescription"))
                            );
                            game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));
                            game.SetStatus(reader.GetString(reader.GetOrdinal("GameStatus")));

                            var item = new Item(
                                reader.GetString(reader.GetOrdinal("ItemName")),
                                game,
                                (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                reader.GetString(reader.GetOrdinal("Description"))
                            );
                            item.SetItemId(reader.GetInt32(reader.GetOrdinal("ItemId")));
                            item.SetIsListed(reader.GetBoolean(reader.GetOrdinal("IsListed")));

                            listings.Add(item);
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }

            return listings;
        }

        public List<Item> GetListingsByGame(Game game)
        {
            var listings = new List<Item>();
            const string query = @"
                SELECT 
                    i.ItemId,
                    i.ItemName,
                    i.Price,
                    i.Description,
                    i.IsListed
                FROM Items i
                WHERE i.CorrespondingGameId = @GameId AND i.IsListed = 1
                ORDER BY i.Price DESC";

            try
            {
                using (var command = new SqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@GameId", game.GameId);
                    OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new Item(
                                reader.GetString(reader.GetOrdinal("ItemName")),
                                game,
                                (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                reader.GetString(reader.GetOrdinal("Description"))
                            );
                            item.SetItemId(reader.GetInt32(reader.GetOrdinal("ItemId")));
                            item.SetIsListed(reader.GetBoolean(reader.GetOrdinal("IsListed")));

                            listings.Add(item);
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }

            return listings;
        }

        public void AddListing(Item item)
        {
            const string query = @"
                UPDATE Items 
                SET IsListed = 1
                WHERE ItemId = @ItemId";

            try
            {
                using (var command = new SqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@ItemId", item.ItemId);
                    OpenConnection();
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        public void RemoveListing(Item item)
        {
            const string query = @"
                UPDATE Items 
                SET IsListed = 0
                WHERE ItemId = @ItemId";

            try
            {
                using (var command = new SqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@ItemId", item.ItemId);
                    OpenConnection();
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        public void UpdateListing(Item item)
        {
            const string query = @"
                UPDATE Items 
                SET ItemName = @ItemName,
                    Price = @Price,
                    Description = @Description
                WHERE ItemId = @ItemId";

            try
            {
                using (var command = new SqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@ItemId", item.ItemId);
                    command.Parameters.AddWithValue("@ItemName", item.ItemName);
                    command.Parameters.AddWithValue("@Price", item.Price);
                    command.Parameters.AddWithValue("@Description", item.Description);
                    
                    OpenConnection();
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        public List<Item> GetInventoryItems(Game game)
        {
            var items = new List<Item>();
            const string query = @"
                SELECT 
                    i.ItemId,
                    i.ItemName,
                    i.Price,
                    i.Description,
                    i.IsListed
                FROM Items i
                JOIN UserInventory ui ON i.ItemId = ui.ItemId
                WHERE ui.GameId = @GameId AND ui.UserId = @UserId";

            try
            {
                using (var command = new SqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@GameId", game.GameId);
                    command.Parameters.AddWithValue("@UserId", GetCurrentUser().UserId);
                    OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new Item(
                                reader.GetString(reader.GetOrdinal("ItemName")),
                                game,
                                (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                reader.GetString(reader.GetOrdinal("Description"))
                            );
                            item.SetItemId(reader.GetInt32(reader.GetOrdinal("ItemId")));
                            item.SetIsListed(reader.GetBoolean(reader.GetOrdinal("IsListed")));
                            items.Add(item);
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }

            return items;
        }

        public List<Item> GetAllInventoryItems(User user)
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
                    g.Genre,
                    g.Description as GameDescription,
                    g.Price as GamePrice,
                    g.Status as GameStatus
                FROM Items i
                JOIN UserInventory ui ON i.ItemId = ui.ItemId
                JOIN Games g ON ui.GameId = g.GameId
                WHERE ui.UserId = @UserId";

            try
            {
                using (var command = new SqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@UserId", user.UserId);
                    OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = new Game(
                                reader.GetString(reader.GetOrdinal("GameTitle")),
                                (float)reader.GetDouble(reader.GetOrdinal("GamePrice")),
                                reader.GetString(reader.GetOrdinal("Genre")),
                                reader.GetString(reader.GetOrdinal("GameDescription"))
                            );
                            game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));
                            game.SetStatus(reader.GetString(reader.GetOrdinal("GameStatus")));

                            var item = new Item(
                                reader.GetString(reader.GetOrdinal("ItemName")),
                                game,
                                (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                reader.GetString(reader.GetOrdinal("Description"))
                            );
                            item.SetItemId(reader.GetInt32(reader.GetOrdinal("ItemId")));
                            item.SetIsListed(reader.GetBoolean(reader.GetOrdinal("IsListed")));
                            items.Add(item);
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }

            return items;
        }

        public void AddInventoryItem(Game game, Item item, User user)
        {
            const string query = @"
                INSERT INTO UserInventory (UserId, GameId, ItemId)
                VALUES (@UserId, @GameId, @ItemId)";

            try
            {
                using (var command = new SqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@UserId", user.UserId);
                    command.Parameters.AddWithValue("@GameId", game.GameId);
                    command.Parameters.AddWithValue("@ItemId", item.ItemId);
                    OpenConnection();
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        public void RemoveInventoryItem(Game game, Item item, User user)
        {
            const string query = @"
                DELETE FROM UserInventory 
                WHERE UserId = @UserId AND GameId = @GameId AND ItemId = @ItemId";

            try
            {
                using (var command = new SqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@UserId", user.UserId);
                    command.Parameters.AddWithValue("@GameId", game.GameId);
                    command.Parameters.AddWithValue("@ItemId", item.ItemId);
                    OpenConnection();
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        public List<Game> GetGames()
        {
            var games = new List<Game>();
            const string query = @"
                SELECT 
                    GameId,
                    Title,
                    Price,
                    Genre,
                    Description,
                    Status
                FROM Games
                ORDER BY Title";

            try
            {
                using (var command = new SqlCommand(query, GetConnection()))
                {
                    OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = new Game(
                                reader.GetString(reader.GetOrdinal("Title")),
                                (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                reader.GetString(reader.GetOrdinal("Genre")),
                                reader.GetString(reader.GetOrdinal("Description"))
                            );
                            game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));
                            game.SetStatus(reader.GetString(reader.GetOrdinal("Status")));
                            games.Add(game);
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }

            return games;
        }

        public Game GetGameById(int gameId)
        {
            const string query = @"
                SELECT 
                    GameId,
                    Title,
                    Price,
                    Genre,
                    Description,
                    Status
                FROM Games
                WHERE GameId = @GameId";

            try
            {
                using (var command = new SqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@GameId", gameId);
                    OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var game = new Game(
                                reader.GetString(reader.GetOrdinal("Title")),
                                (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                reader.GetString(reader.GetOrdinal("Genre")),
                                reader.GetString(reader.GetOrdinal("Description"))
                            );
                            game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));
                            game.SetStatus(reader.GetString(reader.GetOrdinal("Status")));
                            return game;
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }

            return null;
        }

        public List<Item> GetUserInventory(int userId)
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
                    g.Genre,
                    g.Description as GameDescription,
                    g.Price as GamePrice,
                    g.Status as GameStatus
                FROM UserInventory ui
                JOIN Items i ON ui.ItemId = i.ItemId
                JOIN Games g ON ui.GameId = g.GameId
                WHERE ui.UserId = @UserId
                ORDER BY g.Title, i.Price";

            try
            {
                System.Diagnostics.Debug.WriteLine($"Executing GetUserInventory query for userId: {userId}");
                using (var command = new SqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    OpenConnection();
                    System.Diagnostics.Debug.WriteLine("Connection opened successfully");
                    
                    using (var reader = command.ExecuteReader())
                    {
                        System.Diagnostics.Debug.WriteLine("Query executed successfully");
                        while (reader.Read())
                        {
                            System.Diagnostics.Debug.WriteLine($"Found item: {reader.GetString(reader.GetOrdinal("ItemName"))}");
                            var game = new Game(
                                reader.GetString(reader.GetOrdinal("GameTitle")),
                                (float)reader.GetDouble(reader.GetOrdinal("GamePrice")),
                                reader.GetString(reader.GetOrdinal("Genre")),
                                reader.GetString(reader.GetOrdinal("GameDescription"))
                            );
                            game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));
                            game.SetStatus(reader.GetString(reader.GetOrdinal("GameStatus")));

                            var item = new Item(
                                reader.GetString(reader.GetOrdinal("ItemName")),
                                game,
                                (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                reader.GetString(reader.GetOrdinal("Description"))
                            );
                            item.SetItemId(reader.GetInt32(reader.GetOrdinal("ItemId")));
                            item.SetIsListed(reader.GetBoolean(reader.GetOrdinal("IsListed")));

                            // Set image path based on game and item name
                            string imagePath = GetItemImagePath(item);
                            item.SetImagePath(imagePath);

                            items.Add(item);
                        }
                        System.Diagnostics.Debug.WriteLine($"Total items found: {items.Count}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetUserInventory: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
            finally
            {
                CloseConnection();
            }

            return items;
        }

        private string GetItemImagePath(Item item)
        {
            try
            {
                string gameTitle = item.Game.Title.ToLower();
                string itemName = item.ItemName.ToLower();

                // Extract the weapon type from the item name (before the | character)
                string weaponType = itemName.Contains("|") ? itemName.Split('|')[0].Trim().ToLower() : itemName.ToLower();
                
                // Return a path to a default image based on the weapon type
                var path = $"ms-appx:///Assets/img/games/cs2/{weaponType}.png";
                System.Diagnostics.Debug.WriteLine($"Generated image path for {itemName}: {path}");
                return path;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetItemImagePath: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return "ms-appx:///Assets/img/games/default-item.png";
            }
        }

        public void AddToUserInventory(int userId, int itemId, int gameId)
        {
            ExecuteStoredProcedure("sp_AddToUserInventory",
                new SqlParameter("@UserId", userId),
                new SqlParameter("@ItemId", itemId),
                new SqlParameter("@GameId", gameId)
            );
        }

        public void RemoveFromUserInventory(int userId, int itemId, int gameId)
        {
            ExecuteStoredProcedure("sp_RemoveFromUserInventory",
                new SqlParameter("@UserId", userId),
                new SqlParameter("@ItemId", itemId),
                new SqlParameter("@GameId", gameId)
            );
        }

        public void TestDatabaseData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Testing database data...");
                
                using (var connection = new SqlConnection(connectionString))
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
                    using (var command = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM UserInventory ui 
                        JOIN Items i ON ui.ItemId = i.ItemId 
                        JOIN Games g ON ui.GameId = g.GameId 
                        WHERE ui.UserId = 1", connection))
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

        public void CheckDatabaseState()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("\n=== Database State Check ===");
                using (var connection = new SqlConnection(connectionString))
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
                    using (var command = new SqlCommand(@"
                        SELECT ui.UserId, ui.GameId, ui.ItemId, u.Username, g.Title as GameTitle, i.ItemName 
                        FROM UserInventory ui
                        JOIN Users u ON ui.UserId = u.UserId
                        JOIN Games g ON ui.GameId = g.GameId
                        JOIN Items i ON ui.ItemId = i.ItemId", connection))
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
    }
} 