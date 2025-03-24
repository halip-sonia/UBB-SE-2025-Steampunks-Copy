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
                        bool hasGames = false;
                        while (reader.Read())
                        {
                            hasGames = true;
                            var game = new Game(
                                reader.GetString(reader.GetOrdinal("Title")),
                                (float)reader.GetDouble(reader.GetOrdinal("Price")),
                                reader.GetString(reader.GetOrdinal("Genre")),
                                reader.GetString(reader.GetOrdinal("Description"))
                            );
                            game.SetGameId(reader.GetInt32(reader.GetOrdinal("GameId")));
                            games.Add(game);
                        }

                        if (!hasGames)
                        {
                            // If no games exist, create test games
                            CloseConnection();
                            InsertTestGames();
                            return GetAllGames(); // Recursive call to get the newly inserted games
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
                        
                        // If no users exist, create test users
                        CloseConnection();
                        InsertTestUsers();
                        return GetCurrentUser(); // Recursive call to get the newly inserted user
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
        public List<GameTrade> GetActiveGameTrades()
        {
            var trades = new List<GameTrade>();
            var currentUser = GetCurrentUser();

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
                command.Parameters.AddWithValue("@UserId", currentUser.UserId);

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

        public void AcceptTrade(int tradeId)
        {
            var currentUser = GetCurrentUser();
            
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

        public List<GameTrade> GetTradeHistory()
        {
            var trades = new List<GameTrade>();
            var currentUser = GetCurrentUser();

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
                command.Parameters.AddWithValue("@UserId", currentUser.UserId);

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
                    g.Price as GamePrice
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
                    i.IsListed,
                    g.GameId,
                    g.Title as GameTitle,
                    g.Genre,
                    g.Description as GameDescription,
                    g.Price as GamePrice
                FROM Items i
                JOIN Games g ON i.CorrespondingGameId = g.GameId
                WHERE i.IsListed = 1 AND g.GameId = @GameId
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
    }
} 