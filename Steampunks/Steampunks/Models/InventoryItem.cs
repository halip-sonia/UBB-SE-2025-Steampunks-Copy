using System;

namespace Steampunks.Models
{
    public class InventoryItem
    {
        private int itemID;
        private string itemName;
        private Game correspondingGame;
        private int marketPrice;
        private string description;
        private bool isListed;

        public string getItemName()
        {
            return itemName;
        }

        public void setItemName(string itemName)
        {
            this.itemName = itemName;
        }

        public Game getCorrespondingGame()
        {
            return correspondingGame;
        }

        public void setCorrespondingGame(Game game)
        {
            this.correspondingGame = game;
        }

        public string getItemDescription()
        {
            return description;
        }

        public void setItemDescription(string description)
        {
            this.description = description;
        }
    }
} 