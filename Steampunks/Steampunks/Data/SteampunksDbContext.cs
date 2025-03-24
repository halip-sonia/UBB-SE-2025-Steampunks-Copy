//using Microsoft.EntityFrameworkCore;
//using Steampunks.Domain.Entities;

//namespace Steampunks.Data
//{
//    public class SteampunksDbContext : DbContext
//    {
//        public SteampunksDbContext(DbContextOptions<SteampunksDbContext> options)
//            : base(options)
//        {
//        }

//        public DbSet<User> Users { get; set; } = null!;
//        public DbSet<Skin> Skins { get; set; } = null!;
//       // public DbSet<InventoryItem> InventoryItems { get; set; } = null!;
//        public DbSet<Trade> Trades { get; set; } = null!;
//        public DbSet<TradeItem> TradeItems { get; set; } = null!;

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            modelBuilder.Entity<Trade>()
//                .HasOne(t => t.Sender)
//                .WithMany(u => u.SentTrades)
//                .HasForeignKey(t => t.SenderId)
//                .OnDelete(DeleteBehavior.Restrict);

//            modelBuilder.Entity<Trade>()
//                .HasOne(t => t.Receiver)
//                .WithMany(u => u.ReceivedTrades)
//                .HasForeignKey(t => t.ReceiverId)
//                .OnDelete(DeleteBehavior.Restrict);

//            modelBuilder.Entity<InventoryItem>()
//                .HasOne(i => i.User)
//                .WithMany(u => u.Inventory)
//                .HasForeignKey(i => i.UserId);

//            modelBuilder.Entity<TradeItem>()
//                .HasOne(ti => ti.Trade)
//                .WithMany(t => t.OfferedItems)
//                .HasForeignKey(ti => ti.TradeId)
//                .OnDelete(DeleteBehavior.Cascade);
//        }
//    }
//} 