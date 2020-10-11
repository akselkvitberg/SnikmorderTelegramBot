using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Snikmorder.Core.Resources;
using Snikmorder.Core.Services;

namespace Snikmorder.Core.Models
{
    public class GameService
    {
        private readonly GameContext _gameContext;
        private readonly IPlayerRepository _playerRepository;
        private readonly ITelegramSender _sender;

        public GameService(GameContext gameContext, IPlayerRepository playerRepository, ITelegramSender sender)
        {
            _gameContext = gameContext;
            _playerRepository = playerRepository;
            _sender = sender;
        }

        public async Task StartGame()
        {
            var game = await _gameContext.Games.FirstOrDefaultAsync();
            game.State = GameState.Started;
            await _gameContext.SaveChangesAsync();

            var allWaitingPlayers = await _playerRepository.GetAllPlayersInState(PlayerState.WaitingForGameStart);

            var list1 = allWaitingPlayers.OrderBy(x => Guid.NewGuid()).ToList();
            var list2 = list1.Skip(1).Concat(list1.Take(1)); // shift list by 1

            // zip takes each element from list1 and joins it with the corresponding item from list2.
            var combine = list1.Zip(list2);
            foreach (var tuple in combine)
            {
                tuple.First.TargetId = tuple.Second.TelegramUserId;
                tuple.First.State = PlayerState.Active;
                await _sender.SendImage(tuple.First, string.Format(Messages.FirstTarget, tuple.Second.PlayerName), tuple.Second.PictureId);
            }

            await _playerRepository.Save();
        }

        public async Task EndWithWinners(Player player1, Player player2)
        {
            var game = await _gameContext.Games.FirstOrDefaultAsync();
            game.State = GameState.Ended;
            await _gameContext.SaveChangesAsync();

            player1.State = PlayerState.Winner;
            player2.State = PlayerState.Winner;

            var allPlayersInGame = await _playerRepository.GetAllPlayersInGame();

            foreach (var player in allPlayersInGame)
            {
                await _sender.SendMessage(player, $"Spillet er over! {player1.PlayerName} og {player2.PlayerName} er vinnerne!");
            }

            await _sender.SendMessage(player2, "Gratulerer! Du kom på andreplass!");
            await _sender.SendMessage(player1, "Gratulerer! Du kom på førsteplass!");
        }

        public async Task<Game> GetGame()
        {
            var game = await _gameContext.Games.FirstOrDefaultAsync();
            if (game != null)
            {
                return game;
            }
            var entity = new Game()
            {
                Id = Guid.NewGuid()
            };
            await _gameContext.Games.AddAsync(entity);
            await _gameContext.SaveChangesAsync();
            return entity;
        }

        public async Task SetState(GameState state)
        {
            var game = await _gameContext.Games.FirstOrDefaultAsync();
            if (game != null)
            {
                game.State = state;
                await _gameContext.SaveChangesAsync();
            }
        }
    }

    public class Game
    {
        public Guid Id { get; set; }
        public GameState State { get; set; }
    }

    public enum GameState
    {
        NotStarted,
        PreStart,
        Started,
        Ended,
    }
}