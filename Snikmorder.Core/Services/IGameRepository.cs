using Snikmorder.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Snikmorder.Core.Services
{
    public interface IGameRepository
    {
        Task AddPlayer(Player player);

        Task<List<Player>> GetAllPlayersInState(PlayerState state);

        Task<List<Player>> GetAllPlayersActive();
        Task<List<Player>> GetAllPlayersInGame();
        Task<Player?> GetHunter(long telegramId);
        Task<Player?> GetPlayer(long telegramUserId);
        Task<Player?> GetPlayerByAgentName(string agentName);

        Task<Player?> GetPlayerApprovedBy(long adminId);

        Task Reset();
        Task Save();

        Task SetGameState(GameState state);
        Task<GameState> GetGameState();
        Task<List<Contact>> GetAdmins();
        Task AddAdmin(Contact messageContact);
        Task<bool> IsAdmin(long userId);
    }
}