using System;
using Snikmorder.Core.Models;

namespace Snikmorder.Core.Services
{
    public class PlayerRepository
    {
        public Player? GetPlayer(int telegramId)
        {
            return new Player()
            {
                
            };
        }

        public void AddPlayer(Player player)
        {
            throw new NotImplementedException();
        }

        public void Save(Player player)
        {
            throw new NotImplementedException();
        }

        public Player? GetPlayerByAgentName(string agentName)
        {
            return new Player();
        }
    }
}