using System.Collections.Generic;
using Snikmorder.Core.Models;
using Snikmorder.Core.Resources;
using Telegram.Bot.Types;
using Game = Snikmorder.Core.Models.Game;

namespace Snikmorder.Core.Services
{
    public class ApprovalStateMachine
    {
        private readonly ITelegramSender _sender;
        private readonly PlayerRepository _playerRepository;
        private readonly Game _game;
        Queue<Player> playersWaitingForApproval = new Queue<Player>();

        Dictionary<int, Player> ApprovalState = new Dictionary<int, Player>();

        public ApprovalStateMachine(ITelegramSender sender, PlayerRepository playerRepository, Game game)
        {
            _sender = sender;
            _playerRepository = playerRepository;
            _game = game;
        }

        public bool IsFromAdmin(Message message)
        {
            if (message.From.Id == 0)
            {
                return true;
            }
            // Todo: Detect if user is admin - stored in db?
            return false;
        }

        public void HandleAdminMessage(Message message)
        {
            // Handle messages such as "approve application"

            var fromId = message.From.Id;

            #if DEBUG
            if (message.Text == "/all")
            {
                foreach (var player in playersWaitingForApproval)
                {
                    player.State = PlayerState.WaitingForGameStart;
                    _sender.SendMessage(player, string.Format(Messages.ApplicationApproved, player.AgentName));
                }
                playersWaitingForApproval.Clear();
            }
            #endif

            var text = message.Text.ToLower();
            if (ApprovalState.ContainsKey(fromId))
            {
                // Approval status
                var player = ApprovalState[fromId];

                if (text == "/godkjenn")
                {
                    player.State = PlayerState.WaitingForGameStart;
                    _sender.SendMessage(player, string.Format(Messages.ApplicationApproved, player.AgentName));
                    GetNextForApproval(fromId);
                }
                else if (text == "/forkast")
                {
                    player.State = PlayerState.Started;
                    _sender.SendMessage(player, Messages.ApplicationNotApproved);
                    GetNextForApproval(fromId);
                }
                else
                {
                    _sender.SendMessage(fromId, Messages.UnknownApprovalMessage);
                }
            }
            else
            {
                switch (text)
                {
                    case "/hjelp":
                        _sender.SendMessage(fromId, Messages.ApprovalHelp);
                        return;
                    case "/status" when !_game.IsStarted:
                        int waitingPlayers = _playerRepository.GetWaitingPlayerCount();
                        _sender.SendMessage(fromId, $"Det er {waitingPlayers} agenter som venter på start.\n{playersWaitingForApproval.Count} agenter som venter på godkjenning.");
                        return;
                    case "/status":
                        int activePlayers = _playerRepository.GetActivePlayerCount();
                        int deadPlayers = _playerRepository.GetDeadPlayerCount();
                        _sender.SendMessage(fromId, $"Det er {activePlayers} agenter i spill.\nDet er {deadPlayers} døde agenter.");
                        return;
                    case "/neste":
                        GetNextForApproval(fromId);
                        return;
                    case "/begynn" when playersWaitingForApproval.Count != 0:
                        _sender.SendMessage(fromId, $"Det er fremdeles {playersWaitingForApproval.Count} agenter som må godkjennes");
                        return;
                    case "/begynn":
                        int waitingPlayers2 = _playerRepository.GetWaitingPlayerCount();
                        _sender.SendMessage(fromId, $"Det er {waitingPlayers2} agenter som venter på start.\nEr du sikker på at du vil starte spillet?\n/Ja - Starter spillet\n/Nei - utsett start");
                        _game.PreStart = true;
                        return;
                    case "/ja" when _game.PreStart:
                        _sender.SendMessage(fromId, $"Spillet er startet!");
                        _game.StartGame();
                        return;
                    case "/nei" when _game.PreStart:
                        _game.PreStart = false;
                        return;
                }
            }
        }

        private void GetNextForApproval(in int fromId)
        {
            if (playersWaitingForApproval.TryDequeue(out var player))
            {
                ApprovalState[fromId] = player;
                _sender.SendImage(fromId, $"Navn: {player.PlayerName}\nAgent {player.AgentName}\n/godkjenn eller \n/forkast", player.PictureId);
            }
            else
            {
                ApprovalState.Remove(fromId);
                _sender.SendMessage(fromId, "Det er ingen agenter til godkjenning.\nSend /begynn for å starte spillet.");
            }
        }

        public void AddApplication(Player player)
        {
            playersWaitingForApproval.Enqueue(player);
        }
    }
}