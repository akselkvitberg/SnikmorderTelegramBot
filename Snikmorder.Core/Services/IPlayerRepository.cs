using Snikmorder.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snikmorder.Core.Services
{
    public interface IPlayerRepository
    {
        Task AddPlayer(Player player);
        int GetActivePlayerCount();
        List<Player> GetAllPlayersActive();
        List<Player> GetAllPlayersInGame();
        List<Player> GetAllWaitingPlayers();
        int GetDeadPlayerCount();
        Player? GetHunter(long telegramId);
        Player? GetPlayer(int telegramUserId);
        Player? GetPlayerByAgentName(string agentName);
        int GetWaitingPlayerCount();
        void Reset();
        void Save(Player player);
    }
}