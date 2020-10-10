using System;
using System.Collections.Generic;
using System.Linq;
using Snikmorder.Core.Resources;
using Snikmorder.Core.Services;

namespace Snikmorder.Core.Models
{
    public class Game
    {
        private readonly PlayerRepository _playerRepository;
        private readonly ITelegramSender _sender;

        public Game(PlayerRepository playerRepository, ITelegramSender sender)
        {
            _playerRepository = playerRepository;
            _sender = sender;
        }

        public GameState State { get; set; }

        public void StartGame()
        {
            State = GameState.Started;

            var allWaitingPlayers = _playerRepository.GetAllWaitingPlayers();

            var list1 = allWaitingPlayers.OrderBy(x => Guid.NewGuid()).ToList();
            var list2 = list1.Skip(1).Concat(list1.Take(1)); // shift list by 1

            // zip takes each element from list1 and joins it with the corresponding item from list2.
            var combine = list1.Zip(list2);
            foreach (var tuple in combine)
            {
                tuple.First.TargetId = tuple.Second.TelegramUserId;
                tuple.First.State = PlayerState.Active;
                _sender.SendImage(tuple.First, string.Format(Messages.FirstTarget, tuple.Second.PlayerName), tuple.Second.PictureId);
            }
        }

        public void EndWithWinners(Player player1, Player player2)
        {
            player1.State = PlayerState.Winner;
            player2.State = PlayerState.Winner;

            _sender.SendMessage(player1, "Gratulerer! Du kom på førsteplass!");
            _sender.SendMessage(player2, "Gratulerer! Du kom på andreplass!");

            var allPlayersInGame = _playerRepository.GetAllPlayersInGame();

            foreach (var player in allPlayersInGame)
            {
                _sender.SendMessage(player, $"Spillet er over! {player1.PlayerName} og {player2.PlayerName} er vinnerne!");
            }
        }
    }

    public enum GameState
    {
        NotStarted,
        PreStart,
        Started,
        Ended,
    }
}