using System;
using System.Collections.Generic;
using Steampunks.Domain.Entities;
using Steampunks.DataLink;

namespace Steampunks.Repository.GameRepo
{
    public class GameRepository : IGameRepository
    {
        private readonly DatabaseConnector _databaseConnector;

        public GameRepository()
        {
            _databaseConnector = new DatabaseConnector();
        }

        /// <summary>
        /// Retrieves all game records from the database.
        /// </summary>
        /// <returns>
        /// A list of Game entities representing all games in the database.
        /// </returns>
        public List<Game> GetGames()
        {
            return _databaseConnector.GetGames();
        }

        /// <summary>
        /// Retrieves a single game entity based on its unique identifier.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game to retrieve.</param>
        /// <returns>
        /// A Game object if a game with the specified <paramref name="gameId"/> exists; otherwise, null.
        /// </returns>
        public Game GetGameById(int gameId)
        {
            return _databaseConnector.GetGameById(gameId);
        }


    }

    public interface IGameRepository
    {
        List<Game> GetGames();
        Game GetGameById(int gameId);
    }
}