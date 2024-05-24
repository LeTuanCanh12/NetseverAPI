using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebApiApp.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions options) : base(options) { }


        #region DbSet
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatInfo> ChatInfos { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NguoiDung>(entity => {
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.Property(e => e.HoTen).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.HasKey(e => new { e.SenderId, e.ReceiverId, e.ChatId });

                entity.HasOne(e => e.Sender).WithMany(e => e.ChatsSent).HasForeignKey(e => e.SenderId).HasConstraintName("FK_Sender").OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Receiver).WithMany(e => e.ChatsReceived).HasForeignKey(e => e.ReceiverId).HasConstraintName("FK_Receiver").OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChatInfo>(entity =>
            {
                
            });
        }
    }
}
