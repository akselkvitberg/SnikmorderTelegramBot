using Snikmorder.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snikmorder.Core.Services
{
    public interface IPlayerRepository
    {
        Task AddPlayer(Player player);

        Task<List<Player>> GetAllPlayersInState(PlayerState state);

        Task<List<Player>> GetAllPlayersActive();
        Task<List<Player>> GetAllPlayersInGame();
        Task<Player?> GetHunter(long telegramId);
        Task<Player?> GetPlayer(int telegramUserId);
        Task<Player?> GetPlayerByAgentName(string agentName);

        Task<Player?> GetPlayerApprovedBy(int adminId);

        Task Reset();
        Task Save();
    }
}