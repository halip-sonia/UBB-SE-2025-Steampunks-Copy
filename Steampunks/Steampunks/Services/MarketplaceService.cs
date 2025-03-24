using System;
using System.Collections.Generic;
using Steampunks.Domain.Entities;

namespace Steampunks.Services
{
    public class MarketplaceService
    {
        private readonly MarketplaceRepository _marketplaceRepo;
        private readonly User _user;

        public MarketplaceService(MarketplaceRepository marketplaceRepo, User user)
        {
            _marketplaceRepo = marketplaceRepo ?? throw new ArgumentNullException(nameof(marketplaceRepo));
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        public List<Item> getAllListings()
        {
            return _marketplaceRepo.GetAllListings();
        }

        public List<Item> getListingsByGame(Game game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            return _marketplaceRepo.GetListingsByGame(game);
        }

        public void addListing(Game game, Item item)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _marketplaceRepo.AddListing(game, item);
        }

        public void removeListing(Game game, Item item)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _marketplaceRepo.RemoveListing(game, item);
        }

        public void updateListing(Game game, Item item)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _marketplaceRepo.UpdateListing(game, item);
        }
    }
}
