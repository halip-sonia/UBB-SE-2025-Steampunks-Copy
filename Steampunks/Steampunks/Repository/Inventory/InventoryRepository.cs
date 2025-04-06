namespace Steampunks.Repository.Inventory
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Steampunks.DataLink;
    using Steampunks.Domain.Entities;
    using Steampunks.Utils;

    /// <summary>
    /// Provides functionality to manage inventory items associated with users and games.
    /// </summary>
    public class InventoryRepository : IInventoryRepository
    {
        private readonly string connectionString;
        private SqlConnection? connection;

        private DatabaseConnector? dataBaseConnector;
        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryRepository"/> class.
        /// </summary>
        public InventoryRepository()
        {
            this.dataBaseConnector = new DatabaseConnector();
        }

        /// <summary>
        /// Retrieves a list of items from a specific game's inventory.
        /// </summary>
        /// <param name="game">The game whose inventory items are to be retrieved.</param>
        /// <returns>A list of <see cref="Item"/> objects associated with the specified game.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="game"/> is null.</exception>.
        public async Task<List<Item>> GetInventoryItemsAsync(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

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
                using (var command = new SqlCommand(query, this.dataBaseConnector.GetConnection()))
                {
                    command.Parameters.AddWithValue("@GameId", game.GameId);
                    command.Parameters.AddWithValue("@UserId", this.dataBaseConnector.GetCurrentUser().UserId);
                    await this.dataBaseConnector.OpenConnectionAsync();
                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
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
                this.dataBaseConnector.CloseConnection();
            }

            return items;
        }

        /// <summary>
        /// Retrieves all inventory items associated with a specific user across all games.
        /// </summary>
        /// <param name="user">The user whose inventory is to be retrieved.</param>
        /// <returns>A list of all <see cref="Item"/> objects belonging to the specified user.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="user"/> is null.</exception>.
        public async Task<List<Item>> GetAllInventoryItemsAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

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
                using (var command = new SqlCommand(query, this.dataBaseConnector.GetConnection()))
                {
                    command.Parameters.AddWithValue("@UserId", user.UserId);
                    this.dataBaseConnector.OpenConnectionAsync();
                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
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
                this.dataBaseConnector.CloseConnection();
            }

            return items;
        }

        /// <summary>
        /// Adds an item to a specific game's inventory for a user.
        /// </summary>
        /// <param name="game">The game to which the item is to be added.</param>
        /// <param name="item">The item to be added.</param>
        /// <param name="user">The user who is adding the item to their inventory.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="game"/>, <paramref name="item"/>, or <paramref name="user"/> is null.
        /// </exception>
        /// <returns>AddInventoryItemAsync returns <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddInventoryItemAsync(Game game, Item item, User user)
        {
            ArgumentNullException.ThrowIfNull(game);

            ArgumentNullException.ThrowIfNull(item);

            ArgumentNullException.ThrowIfNull(user);

            const string query = @"
                INSERT INTO UserInventory (UserId, GameId, ItemId)
                VALUES (@UserId, @GameId, @ItemId)";

            try
            {
                using (var command = new SqlCommand(query, this.dataBaseConnector.GetConnection()))
                {
                    command.Parameters.AddWithValue("@UserId", user.UserId);
                    command.Parameters.AddWithValue("@GameId", game.GameId);
                    command.Parameters.AddWithValue("@ItemId", item.ItemId);
                    await this.dataBaseConnector.OpenConnectionAsync();
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                this.dataBaseConnector.CloseConnection();
            }
        }

        /// <summary>
        /// Removes an item from a specific game's inventory for a user.
        /// </summary>
        /// <param name="game">The game from which the item is to be removed.</param>
        /// <param name="item">The item to be removed.</param>
        /// <param name="user">The user whose inventory the item is being removed from.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="game"/>, <paramref name="item"/>, or <paramref name="user"/> is null.
        /// </exception>
        /// <returns>RemoveInventoryItemAsync returns <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RemoveInventoryItemAsync(Game game, Item item, User user)
        {
            ArgumentNullException.ThrowIfNull(game);

            ArgumentNullException.ThrowIfNull(item);

            ArgumentNullException.ThrowIfNull(user);

            const string query = @"
                DELETE FROM UserInventory 
                WHERE UserId = @UserId AND GameId = @GameId AND ItemId = @ItemId";

            try
            {
                using (var command = new SqlCommand(query, this.dataBaseConnector.GetConnection()))
                {
                    command.Parameters.AddWithValue("@UserId", user.UserId);
                    command.Parameters.AddWithValue("@GameId", game.GameId);
                    command.Parameters.AddWithValue("@ItemId", item.ItemId);
                    this.dataBaseConnector.OpenConnectionAsync();
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                this.dataBaseConnector.CloseConnection();
            }
        }
    }
}