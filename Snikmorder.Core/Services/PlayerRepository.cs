using System;
using System.Collections.Generic;
using System.Linq;
using Snikmorder.Core.Models;

namespace Snikmorder.Core.Services
{
    public class PlayerRepository
    {
        List<Player> players = new List<Player>();

        public Player? GetPlayer(int telegramUserId)
        {
            return players.FirstOrDefault(x => x.TelegramUserId == telegramUserId);
        }

        public void AddPlayer(Player player)
        {
            players.Add(player);
        }

        public void Save(Player player)
        {
            // not needed in this implementation
        }

        public Player? GetPlayerByAgentName(string agentName)
        {
            return players.FirstOrDefault(x => x.AgentName?.ToLower() == agentName);
        }

        public int GetWaitingPlayerCount()
        {
            return players.Count(x => x.State == PlayerState.WaitingForGameStart);
        }

        public int GetActivePlayerCount()
        {
            return players.Count(x => x.State == PlayerState.Active);

        }

        public int GetDeadPlayerCount()
        {
            return players.Count(x => x.State == PlayerState.Killed);

        }

        public List<Player> GetAllWaitingPlayers()
        {
            return players.Where(x => x.State == PlayerState.WaitingForGameStart).ToList();
        }

        public Player? GetHunter(long telegramId)
        {
            return players.FirstOrDefault(x => x.TargetId == telegramId);
        }
    }
}