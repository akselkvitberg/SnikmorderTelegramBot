using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Snikmorder.Core.Models;
using Telegram.Bot.Types;

namespace Snikmorder.Core.Services
{
    public class GameRepository : IGameRepository
    {
        private readonly GameContext _gameContext;

        public GameRepository(GameContext gameContext)
        {
            _gameContext = gameContext;
        }

        public async Task AddPlayer(Player player)
        {
            await _gameContext.Players.AddAsync(player);
        }

        public async Task<List<Player>> GetAllPlayersInState(PlayerState state)
        {
            return await _gameContext.Players.Where(x => x.State == state).ToListAsync();
        }
        
        public async Task<List<Player>> GetAllPlayersActive()
        {
            return (await _gameContext.Players.ToListAsync()).Where(x => x.IsActive).ToList();
        }

        public async Task<List<Player>> GetAllPlayersInGame()
        {
            return (await _gameContext.Players.ToListAsync()).Where(x => x.State > PlayerState.WaitingForGameStart).ToList();
        }

        public async Task<Player?> GetHunter(long telegramId)
        {
            return (await _gameContext.Players.ToListAsync()).FirstOrDefault(x => x.IsActive && x.TargetId == telegramId);
        }

        public async Task<Player?> GetPlayer(int telegramUserId)
        {
            return await _gameContext.Players.FirstOrDefaultAsync(x=>x.TelegramUserId == telegramUserId);
        }

        public async Task<Player?> GetPlayerByAgentName(string agentName)
        {
            return (await _gameContext.Players.ToListAsync()).FirstOrDefault(x => x.AgentName?.ToLower() == agentName);
        }

        public async Task<Player> GetPlayerApprovedBy(int adminId)
        {
            return await _gameContext.Players.FirstOrDefaultAsync(x => x.ApprovalId == adminId);
        }

        public async Task Reset()
        {
            foreach (var player in await _gameContext.Players.ToListAsync())
            {
                player.State = PlayerState.Started;
                player.TargetId = 0;
            }

            await _gameContext.SaveChangesAsync();
        }

        public async Task Save()
        {
            await _gameContext.SaveChangesAsync();
        }

        public async Task SetGameState(GameState state)
        {
            var game = await _gameContext.Games.FirstOrDefaultAsync();
            if (game == null)
            {
                game = new Models.Game();
                await _gameContext.Games.AddAsync(game);
            }
            game.State = state;
            await _gameContext.SaveChangesAsync();
        }

        public async Task<GameState> GetGameState()
        {
            var game = await _gameContext.Games.FirstOrDefaultAsync();
            return game?.State ?? GameState.NotStarted;
        }

        public async Task<List<Contact>> GetAdmins()
        {
            return await _gameContext.Admins.ToListAsync();
        }

        public async Task AddAdmin(Contact messageContact)
        {
            await _gameContext.Admins.AddAsync(messageContact);
            await _gameContext.SaveChangesAsync();
        }

        public async Task<bool> IsAdmin(int userId)
        {
            return await _gameContext.Admins.FirstOrDefaultAsync(x => x.UserId == userId) != null;
        }
    }
}