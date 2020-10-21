using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Snikmorder.Core.Resources;
using Snikmorder.Core.Services;
using Telegram.Bot.Types;

namespace Snikmorder.Core.Models
{
    public class GameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly ITelegramSender _sender;

        public GameService(IGameRepository gameRepository, ITelegramSender sender)
        {
            _gameRepository = gameRepository;
            _sender = sender;
        }

        public async Task StartGame()
        {
            await _gameRepository.SetGameState(GameState.Started);//_gameContext.Games.FirstOrDefaultAsync();

            var allWaitingPlayers = await _gameRepository.GetAllPlayersInState(PlayerState.WaitingForGameStart);

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

            await _gameRepository.Save();
        }

        public async Task EndWithWinners(Player player1, Player player2)
        {
            await _gameRepository.SetGameState(GameState.Ended);
            
            player1.State = PlayerState.Winner;
            player2.State = PlayerState.Winner;

            var allPlayersInGame = await _gameRepository.GetAllPlayersInGame();

            foreach (var player in allPlayersInGame)
            {
                await _sender.SendMessage(player, $"Spillet er over!\n{player1.PlayerName} og {player2.PlayerName} er vinnerne!");
            }

            await _sender.SendMessage(player2, "Gratulerer! Du kom på andreplass!");
            await _sender.SendMessage(player1, "Gratulerer! Du kom på førsteplass!");
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