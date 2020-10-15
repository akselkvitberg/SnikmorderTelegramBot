using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Snikmorder.Core.Models;
using Snikmorder.Core.Services;
using Telegram.Bot.Types;
using Game = Snikmorder.Core.Models.Game;

namespace Snikmorder.DesktopClient.GameMock
{
    public class MockGameRepository : IGameRepository
    {
        static List<Player> players = new List<Player>();
        static private List<Contact> _admins = new List<Contact>();
        static Game _game = new Game();


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



        public Task SetGameState(GameState state)
        {
            _game.State = state;
            return Task.CompletedTask;
        }

        public Task<GameState> GetGameState()
        {
            return Task.FromResult(_game.State);
        }

        public Task<List<Contact>> GetAdmins()
        {
            return Task.FromResult(_admins);
        }

        public Task AddAdmin(Contact messageContact)
        {
            _admins.Add(messageContact);
            return Task.CompletedTask;
        }

        public Task<bool> IsAdmin(int userId)
        {
            return Task.FromResult(_admins.Any(x => x.UserId == userId));
        }

        public async Task<Player?> GetPlayerByAgentName(string agentName)
        {
            return players.FirstOrDefault(x => x.AgentName?.ToLower() == agentName);
        }

        public async Task<Player> GetPlayerApprovedBy(int adminId)
        {
            return players.FirstOrDefault(x => x.ApprovalId == adminId);
        }

        public async Task<List<Player>> GetAllPlayersInState(PlayerState state)
        {
            return players.Where(x => x.State == state).ToList();
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