using System.Collections.Generic;

namespace SnikmorderTelegramBot.Models
{
    public class Game
    {
        public bool IsStarted { get; set; }
        public ICollection<Player> Players { get; set; }
    }
}