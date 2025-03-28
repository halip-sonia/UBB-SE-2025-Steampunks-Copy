using Steampunks.DataLink;
using Steampunks.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steampunks.Services
{
    public class GameService
    {
        private readonly DatabaseConnector _dbConnector;

        public GameService(DatabaseConnector dbConnector)
        {
            _dbConnector = dbConnector;
        }

        public async Task<List<Game>> GetAllGamesAsync()
        {
            return await Task.Run(() => _dbConnector.GetAllGames());
        }

        public async Task<Game> GetGameByIdAsync(int gameId)
        {
            return await Task.Run(() => _dbConnector.GetGameById(gameId));
        }

        public async Task<bool> UpdateGameAsync(Game game)
        {
            return await Task.Run(() => _dbConnector.UpdateGame(game));
        }
    }
} 