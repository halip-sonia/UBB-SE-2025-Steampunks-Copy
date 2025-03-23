using System;
using System.Collections.Generic;

namespace Steampunks.Models
{
    public enum TradeStatus
    {
        Pending,
        Accepted,
        Declined,
        Cancelled
    }

    public class Trade
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public TradeStatus Status { get; set; }
        public string? Message { get; set; }

        public virtual User Sender { get; set; } = null!;
        public virtual User Receiver { get; set; } = null!;
        public virtual ICollection<TradeItem> OfferedItems { get; set; } = new List<TradeItem>();
        public virtual ICollection<TradeItem> RequestedItems { get; set; } = new List<TradeItem>();
    }

    public class TradeItem
    {
        public int Id { get; set; }
        public int TradeId { get; set; }
        public int InventoryItemId { get; set; }
        public bool IsOffered { get; set; }

        public virtual Trade Trade { get; set; } = null!;
        public virtual InventoryItem InventoryItem { get; set; } = null!;
    }
} 