using System;
using System.Collections.Generic;
using Steampunks.Domain.Entities;
using Steampunks.DataLink;

namespace Steampunks.Repository.GameRepo
{
    public class GameRepository : IGameRepository
    {
        private readonly DatabaseConnector _dbConnector;

        public GameRepository()
        {
            _dbConnector = new DatabaseConnector();
        }

        public List<Game> GetGames()
        {
            return _dbConnector.GetGames();
        }

        public Game GetGameById(int gameId)
        {
            return _dbConnector.GetGameById(gameId);
        }
    }

    public interface IGameRepository
    {
        List<Game> GetGames();
        Game GetGameById(int gameId);
    }
} 