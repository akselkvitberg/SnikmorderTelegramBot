using System.Collections.Generic;

namespace Snikmorder.Core.Models
{
    public class Game
    {
        public bool IsStarted { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
    }
}