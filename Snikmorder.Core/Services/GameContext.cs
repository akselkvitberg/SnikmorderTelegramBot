using Microsoft.EntityFrameworkCore;
using Snikmorder.Core.Models;

namespace Snikmorder.Core.Services
{
    public class GameContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Player>().ToContainer("Players").HasNoDiscriminator().HasKey(o=>o.TelegramUserId);
            modelBuilder.Entity<Game>().ToContainer("Game").HasNoDiscriminator().HasKey(game => game.Id);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseCosmos("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", "snikmorder");
        }
    }
}