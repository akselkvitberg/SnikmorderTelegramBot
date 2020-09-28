using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool IsStarted { get; set; }
        public bool PreStart { get; set; }

        public void StartGame()
        {
            IsStarted = true;

            var allWaitingPlayers = _playerRepository.GetAllWaitingPlayers();

            var list1 = allWaitingPlayers.OrderBy(x => new Guid()).ToList();
            var list2 = allWaitingPlayers.Skip(1).Concat(allWaitingPlayers.Take(1)); // shift list by 1
            
            // zip takes each element from list1 and joins it with the corresponding item from list2.
            var combine = list1.Zip(list2);
            foreach (var tuple in combine)
            {
                tuple.First.Target = tuple.Second;
                tuple.Second.Hunter = tuple.First;
            }

            foreach (var player in allWaitingPlayers)
            {
                player.State = PlayerState.Active;

                _sender.SendImage(player, $"Ditt første mål er:\n{player.Target!.PlayerName}", player.Target.PictureId);
            }
        }
    }
}