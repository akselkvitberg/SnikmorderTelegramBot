using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Snikmorder.Core.Models;
using Snikmorder.Core.Services;

namespace Snikmorder.DesktopClient.GameMock
{
    public class MockPlayerRepository : IPlayerRepository
    {
        List<Player> players = new List<Player>();

        public async Task<Player?> GetPlayer(int telegramUserId)
        {
            return players.FirstOrDefault(x => x.TelegramUserId == telegramUserId);
        }

        public Task AddPlayer(Player player)
        {
            players.Add(player);
            return Task.CompletedTask;
        }

        public async  Task Save()
        {
            // not needed in this implementation
        }

        public async Task<Player?> GetPlayerByAgentName(string agentName)
        {
            return players.FirstOrDefault(x => x.AgentName?.ToLower() == agentName);
        }

        public async Task<int> GetWaitingPlayerCount()
        {
            return players.Count(x => x.State == PlayerState.WaitingForGameStart);
        }

        public async Task<Player> GetPlayerApprovedBy(int adminId)
        {
            return players.FirstOrDefault(x => x.ApprovalId == adminId);
        }

        public async Task<int> GetActivePlayerCountAsync()
        {
            return players.Count(x => x.State == PlayerState.Active);

        }

        public async Task<List<Player>> GetAllPlayersInState(PlayerState state)
        {
            return players.Where(x => x.State == state).ToList();
        }

        public async Task<int> CountAllPlayersInState(PlayerState state)
        {
            return players.Count(x => x.State == state);
        }

        public async Task<int> GetDeadPlayerCount()
        {
            return players.Count(x => x.State == PlayerState.Killed);

        }

        public async Task<List<Player>> GetAllWaitingPlayers()
        {
            return players.Where(x => x.State == PlayerState.WaitingForGameStart).ToList();
        }

        public async Task<List<Player>> GetAllPlayersInGame()
        {
            return players.Where(x => x.State > PlayerState.WaitingForGameStart).ToList();
        }

        public async Task<List<Player>> GetAllPlayersActive()
        {
            return players.Where(x => x.IsActive).ToList();
        }

        public async Task<Player?> GetHunter(long telegramId)
        {
            return players.FirstOrDefault(x => x.IsActive && x.TargetId == telegramId);
        }

        public async Task Reset()
        {
            foreach (var player in players)
            {
                player.State = PlayerState.Started;
                player.TargetId = 0;
            }
        }
    }
}