namespace Steampunks.Domain.Entities
{
    public class Item
    {
        public int ItemId { get; private set; }
        public string ItemName { get; private set; }
        public Game CorrespondingGame { get; private set; }
        public float Price { get; private set; }
        public string Description { get; private set; }
        public bool IsListed { get; private set; }

        private Item() { } // For EF Core

        public Item(string itemName, Game correspondingGame, float price, string description)
        {
            ItemName = itemName;
            CorrespondingGame = correspondingGame;
            Price = price;
            Description = description;
            IsListed = false;
        }

        public string GetItemName()
        {
            return ItemName;
        }

        public void SetItemName(string itemName)
        {
            ItemName = itemName;
        }

        public Game GetCorrespondingGame()
        {
            return CorrespondingGame;
        }

        public void SetCorrespondingGame(Game game)
        {
            CorrespondingGame = game;
        }

        public void SetItemDescription(string description)
        {
            Description = description;
        }
    }
} 