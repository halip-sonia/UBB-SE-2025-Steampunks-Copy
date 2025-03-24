using System;
using System.Collections.Generic;
using Steampunks.Domain.Entities;
using Steampunks.Repository.GameRepo;

namespace Steampunks.Services
{
    public class GameService
    {
        private readonly GameRepository _gameRepo;

        public GameService(GameRepository gameRepo)
        {
            _gameRepo = gameRepo ?? throw new ArgumentNullException(nameof(gameRepo));
        }

        public List<Game> GetGames()
        {
            return _gameRepo.GetGames();
        }

        public Game GetGameById(int gameId)
        {
            if (gameId <= 0)
                throw new ArgumentException("Game ID must be positive", nameof(gameId));

            return _gameRepo.GetGameById(gameId);
        }
    }
} 