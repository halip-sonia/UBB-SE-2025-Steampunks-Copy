using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Steampunks.Domain.Entities;
using Steampunks.Repository.Marketplace;
using Steampunks.ViewModels;

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
            // Sample data for testing
            var cs2 = new Game("Counter-Strike 2", 0, "FPS", "The latest version of Counter-Strike");
            cs2.SetGameId(1);

            var items = new List<Item>
            {
                new Item("Dragon Lore", cs2, 999.99f, "A legendary AWP skin with a dragon design")
                {
                    ItemId = 1,
                    IsListed = true
                },
                new Item("AK-47 | Asiimov", cs2, 49.99f, "A futuristic AK-47 skin with a unique design")
                {
                    ItemId = 2,
                    IsListed = true
                },
                new Item("M4A4 | Howl", cs2, 1299.99f, "A rare and valuable M4A4 skin")
                {
                    ItemId = 3,
                    IsListed = true
                },
                new Item("AWP | Neo-Noir", cs2, 79.99f, "A sleek and modern AWP skin")
                {
                    ItemId = 4,
                    IsListed = true
                },
                new Item("USP-S | Neo-Noir", cs2, 29.99f, "A matching USP-S skin")
                {
                    ItemId = 5,
                    IsListed = true
                }
            };

            // Set custom image paths for each item
            items[0].SetImagePath("ms-appx:///Assets/img/games/cs2/dragon-lore.png");
            items[1].SetImagePath("ms-appx:///Assets/img/games/cs2/ak47-asiimov.png");
            items[2].SetImagePath("ms-appx:///Assets/img/games/cs2/m4a4-howl.png");
            items[3].SetImagePath("ms-appx:///Assets/img/games/cs2/awp-neo-noir.png");
            items[4].SetImagePath("ms-appx:///Assets/img/games/cs2/usp-neo-noir.png");

            return items;
        }

        public List<Item> getListingsByGame(Game game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            return getAllListings().Where(i => i.GetCorrespondingGame().GameId == game.GameId).ToList();
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
