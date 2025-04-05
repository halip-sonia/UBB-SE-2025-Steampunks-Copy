// <copyright file="GameService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// <summary>
//   Service class for accessing and managing Game data.
// </summary>
// Refactored by Team ArtAttack, 2025

namespace Steampunks.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Steampunks.DataLink;
    using Steampunks.Domain.Entities;

    /// <summary>
    /// Provides operations for retrieving and updating game data.
    /// </summary>
    public class GameService : IGameService
    {
        private readonly DatabaseConnector databaseConnector;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameService"/> class.
        /// </summary>
        /// <param name="databaseConnector">Database connector for accessing game data.</param>
        public GameService(DatabaseConnector databaseConnector)
        {
            this.databaseConnector = databaseConnector;
        }

        /// <summary>
        /// Asynchronously retrieves all games from the database.
        /// </summary>
        /// <returns>A list of all games.</returns>
        public async Task<List<Game>> GetAllGamesAsync()
        {
            return await Task.Run(() => this.databaseConnector.GetAllGames());
        }

        /// <summary>
        /// Asynchronously retrieves a game by its ID.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <returns>The game with the specified ID.</returns>
        public async Task<Game> GetGameByIdAsync(int gameId)
        {
            return await Task.Run(() => this.databaseConnector.GetGameByIdAsync(gameId));
        }

        /// <summary>
        /// Asynchronously updates a game's information in the database.
        /// </summary>
        /// <param name="game">The game object to update.</param>
        /// <returns>True if update succeeded; otherwise, false.</returns>
        public async Task<bool> UpdateGameAsync(Game game)
        {
            return await Task.Run(() => this.databaseConnector.UpdateGame(game));
        }
    }
}