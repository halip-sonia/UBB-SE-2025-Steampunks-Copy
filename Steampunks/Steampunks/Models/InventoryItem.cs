using System;

namespace Steampunks.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SkinId { get; set; }
        public DateTime AcquiredDate { get; set; }
        public bool IsTradeble { get; set; }
        public string Condition { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Skin Skin { get; set; } = null!;
    }
} 