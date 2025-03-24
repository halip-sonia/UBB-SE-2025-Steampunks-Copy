using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Steampunks.Models;
using Steampunks.DataLink;

namespace Steampunks.Repository.Marketplace
{
    public class MarketplaceRepository : IMarketplaceRepository
    {
        private readonly DbContext _dbContext;

        public MarketplaceRepository(DbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Game> GetListing(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                throw new ArgumentNullException(nameof(itemId));

            return await _dbContext.Set<Game>().FindAsync(itemId);
        }

        public async Task<List<Game>> GetListings()
        {
            return await _dbContext.Set<Game>().ToListAsync();
        }

        public async Task<Game> CreateListing(Game game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            await _dbContext.Set<Game>().AddAsync(game);
            await _dbContext.SaveChangesAsync();
            return game;
        }

        public async Task<Game> UpdateListing(Game game, string itemId)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (string.IsNullOrEmpty(itemId))
                throw new ArgumentNullException(nameof(itemId));

            var existingGame = await GetListing(itemId);
            if (existingGame == null)
                throw new KeyNotFoundException($"Game with ID {itemId} not found");

            _dbContext.Entry(existingGame).CurrentValues.SetValues(game);
            await _dbContext.SaveChangesAsync();
            return existingGame;
        }

        public async Task DeleteListing(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                throw new ArgumentNullException(nameof(itemId));

            var game = await GetListing(itemId);
            if (game == null)
                throw new KeyNotFoundException($"Game with ID {itemId} not found");

            _dbContext.Set<Game>().Remove(game);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Game> GetCorrespondingGame(Game game, string itemId)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (string.IsNullOrEmpty(itemId))
                throw new ArgumentNullException(nameof(itemId));

            return await _dbContext.Set<Game>()
                .FirstOrDefaultAsync(g => g.Id == itemId);
        }
    }

    public interface IMarketplaceRepository
    {
        Task<Game> GetListing(string itemId);
        Task<List<Game>> GetListings();
        Task<Game> CreateListing(Game game);
        Task<Game> UpdateListing(Game game, string itemId);
        Task DeleteListing(string itemId);
        Task<Game> GetCorrespondingGame(Game game, string itemId);
    }
}