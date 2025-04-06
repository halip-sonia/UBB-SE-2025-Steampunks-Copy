// <copyright file="GameRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// <summary>
//   Service class for accessing and managing Game data.
// </summary>
// Refactored by Team ArtAttack, 2025
namespace Steampunks.Repository.GameRepo
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Steampunks.DataLink;
    using Steampunks.Domain.Entities;

    /// <summary>
    /// Provides data access functionality for <see cref="Game"/> entities.
    /// Communicates with the database through the <see cref="DatabaseConnector"/>.
    /// </summary>
    public class GameRepository : IGameRepository
    {
        /// <summary>
        /// Responsible for executing database operations related to games.
        /// </summary>
        private readonly DatabaseConnector databaseConnector;

        /// <summary>
        /// Initializes a new instance of the <see cref=GameRepository"/> class
        /// and sets up a connection to the database by creating a new <see cref="DatabaseConnector"/>.
        /// </summary>
        public GameRepository()
        {
            this.databaseConnector = new DatabaseConnector();
        }

        /// <summary>
        /// Retrieves all game records from the database.
        /// </summary>
        /// <returns>
        /// A list of Game entities representing all games in the database.
        /// </returns>
        public async Task<List<Game>> GetGamesAsync()
        {
            return await this.GetGamesFromDatabaseAsync();
        }

        /// <summary>
        /// Retrieves a single game entity based on its unique identifier.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game to retrieve.</param>
        /// <returns>
        /// A Game object if a game with the specified <paramref name="gameId"/> exists; otherwise, null.
        /// </returns>
        public async Task<Game> GetGameByIdAsync(int gameId) 
        {
            return await this.GetGameByIdFromDatabaseAsync(gameId);
        }

        /// <summary>
        /// Asynchronously updates a game's information in the database.
        /// </summary>
        /// <param name="game">The game object to update.</param>
        /// <returns>True if update succeeded; otherwise, false.</returns>
        public async Task<bool> UpdateGameAsync(Game game)
        {
            return await this.UpdateGameFromDatabaseAsync(game);
        }

        /// <summary>
        /// Asynchronously retrieves a list of all games from the database, ordered by title.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a list of <see cref="Game"/> objects.</returns>
        public async Task<List<Game>> GetGamesFromDatabaseAsync()
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
                using (var command = new SqlCommand(query, this.databaseConnector.GetConnection()))
                {
                    await this.databaseConnector.OpenConnectionAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
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
                this.databaseConnector.CloseConnection();
            }

            return games;
        }

        /// <summary>
        /// Asynchronously retrieves a specific game from the database based on its unique identifier.
        /// </summary>
        /// <param name="gameId">The ID of the game to retrieve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a <see cref="Game"/> object if found; otherwise, <c>null</c>.
        /// </returns>
        public async Task<Game> GetGameByIdFromDatabaseAsync(int gameId)
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
                using (var command = new SqlCommand(query, this.databaseConnector.GetConnection()))
                {
                    command.Parameters.AddWithValue("@GameId", gameId);
                    await this.databaseConnector.OpenConnectionAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
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
                this.databaseConnector.CloseConnection();
            }

            return null;
        }

        /// <summary>
        /// Asynchronously updates the details of an existing game in the database, excluding the status field.
        /// </summary>
        /// <param name="game">The <see cref="Game"/> object containing updated information.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains <c>true</c> if the update was successful; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> UpdateGameFromDatabaseAsync(Game game)
        {
            const string query = @"
        UPDATE Games
        SET Title = @Title,
            Price = @Price,
            Genre = @Genre,
            Description = @Description
        WHERE GameId = @GameId";

            try
            {
                using (var command = new SqlCommand(query, this.databaseConnector.GetConnection()))
                {
                    command.Parameters.AddWithValue("@GameId", game.GameId);
                    command.Parameters.AddWithValue("@Title", game.Title);
                    command.Parameters.AddWithValue("@Price", game.Price);
                    command.Parameters.AddWithValue("@Genre", game.Genre);
                    command.Parameters.AddWithValue("@Description", game.Description);

                    await this.databaseConnector.OpenConnectionAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            finally
            {
                this.databaseConnector.CloseConnection();
            }
        }



    }

}