namespace Steampunks.Repository.GameRepo
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
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
            // Assuming DatabaseConnector has async support
            return await this.databaseConnector.GetGamesAsync();
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
            return await this.databaseConnector.GetGameByIdAsync(gameId);
        }
    }
}