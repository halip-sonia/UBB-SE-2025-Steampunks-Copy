using System;
using System.Collections.Generic;

namespace Steampunks.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public DateTime RegisterDate { get; set; }
        public virtual ICollection<InventoryItem> Inventory { get; set; } = new List<InventoryItem>();
        public virtual ICollection<Trade> SentTrades { get; set; } = new List<Trade>();
        public virtual ICollection<Trade> ReceivedTrades { get; set; } = new List<Trade>();
    }
} 