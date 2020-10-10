using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Snikmorder.Core.Models;

namespace Snikmorder.Core.Services
{
    public class PlayerRepository : IPlayerRepository
    {

        public PlayerRepository()
        {
        }

        public async Task AddPlayer(Player player)
        {
            throw new NotImplementedException();
        }

        public int GetActivePlayerCount()
        {
            throw new NotImplementedException();
        }

        public List<Player> GetAllPlayersActive()
        {
            throw new NotImplementedException();
        }

        public List<Player> GetAllPlayersInGame()
        {
            throw new NotImplementedException();
        }

        public List<Player> GetAllWaitingPlayers()
        {
            throw new NotImplementedException();
        }

        public int GetDeadPlayerCount()
        {
            throw new NotImplementedException();
        }

        public Player? GetHunter(long telegramId)
        {
            throw new NotImplementedException();
        }

        public Player? GetPlayer(int telegramUserId)
        {
            throw new NotImplementedException();
        }

        public Player? GetPlayerByAgentName(string agentName)
        {
            throw new NotImplementedException();
        }

        public int GetWaitingPlayerCount()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Save(Player player)
        {
            throw new NotImplementedException();
        }
    }
}