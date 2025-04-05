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
        /// Retrieves all game records from the database.
        /// </summary>
        /// <returns>
        /// A list of Game entities representing all games in the database.
        /// </returns>
        List<Game> GetGames();

        /// <summary>
        /// Retrieves a single game entity based on its unique identifier.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game to retrieve.</param>
        /// <returns>
        /// A Game object if a game with the specified <paramref name="gameId"/> exists; otherwise, null.
        /// </returns>
        Game GetGameById(int gameId);
    }
}
