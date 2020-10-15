using Microsoft.EntityFrameworkCore;
using Snikmorder.Core.Models;
using Telegram.Bot.Types;
using Game = Snikmorder.Core.Models.Game;

namespace Snikmorder.Core.Services
{
    public class GameContext : DbContext
    {
        private readonly bool _local;
        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Contact> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Player>().ToContainer("Players").HasNoDiscriminator().HasKey(o=>o.TelegramUserId);
            modelBuilder.Entity<Game>().ToContainer("Game").HasNoDiscriminator().HasKey(game => game.Id);
            modelBuilder.Entity<Contact>().ToContainer("Admins").HasNoDiscriminator().HasKey(a => a.UserId);
            modelBuilder.Entity<Contact>().HasData(new Contact()
            {
                UserId = 49374973,
                FirstName = "Aksel",
                LastName = "Kvitberg",
                PhoneNumber = "40451802",
            });
        }

        public GameContext(bool local)
        {
            _local = local;
        }

        public GameContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (_local)
            {
                optionsBuilder.UseCosmos("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", "snikmorder");
            }
        }

    }
}