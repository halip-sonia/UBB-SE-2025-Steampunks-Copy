namespace Steampunks.Repository.GameRepo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Steampunks.Domain.Entities;

    /// <summary>
    /// Provides data access functionality for <see cref="Game"/> entities.
    /// Communicates with the database through the <see cref="DatabaseConnector"/>.
    /// </summary>
    public interface IGameRepository
    {
        /// <summary>
        /// Asynchronously retrieves all game records from the database.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, with a result of a list of games.
        /// </returns>
        Task<List<Game>> GetGamesAsync();

        /// <summary>
        /// Asynchronously retrieves a single game entity based on its unique identifier.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game.</param>
        /// <returns>
        /// A task with a result of the matching Game, or null if not found.
        /// </returns>
        Task<Game> GetGameByIdAsync(int gameId);

        /// <summary>
        /// Asynchronously updates a game's information in the database.
        /// </summary>
        /// <param name="game">The game object to update.</param>
        /// <returns>True if update succeeded; otherwise, false.</returns>
        Task<bool> UpdateGameAsync(Game game);
    }
}
