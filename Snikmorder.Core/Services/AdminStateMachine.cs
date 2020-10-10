using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Snikmorder.Core.Models;
using Snikmorder.Core.Resources;
using Telegram.Bot.Types;
using Game = Snikmorder.Core.Models.Game;

namespace Snikmorder.Core.Services
{
    public class AdminStateMachine
    {
        private readonly ITelegramSender _sender;
        private readonly PlayerRepository _playerRepository;
        private readonly Game _game;
        Queue<Player> playersWaitingForApproval = new Queue<Player>();

        Dictionary<int, Player> ApprovalState = new Dictionary<int, Player>();

        public AdminStateMachine(ITelegramSender sender, PlayerRepository playerRepository, Game game)
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
            var fromId = message.From.Id;

            if (_game.State == GameState.Started)
            {
                switch (message.Text.ToLower())
                {
                    case "/hjelp":
                        _sender.SendMessage(fromId, "Kommandoer:\n/status - se hvor mange agenter som er i spill\n/oppdrag - se hvilke mål hver agent har");
                        return;
                    case "/status":
                        int activePlayers = _playerRepository.GetActivePlayerCount();
                        int deadPlayers = _playerRepository.GetDeadPlayerCount();
                        _sender.SendMessage(fromId, $"Det er {activePlayers} agenter i spill.\nDet er {deadPlayers} døde agenter.");
                        return;
                    case "/oppdrag":
                        return;
                }
                return;
            }

            if (_game.State == GameState.Ended)
            {
                if (message.Text.ToLower() == "/restart")
                {
                    _game.State = GameState.NotStarted;
                    _playerRepository.Reset();
                }
                return;
            }

            switch (message.Text)
            {
                
            }
            
            
            
            // Handle messages such as "approve application"
            #if DEBUG
            if (message.Text == "/all")
            {
                foreach (var player in playersWaitingForApproval)
                {
                    player.State = PlayerState.WaitingForGameStart;
                    _sender.SendMessage(player, string.Format(Messages.ApplicationApproved, player.AgentName));
                }
                playersWaitingForApproval.Clear();
                _sender.SendMessage(fromId, "Det er ingen agenter til godkjenning.\nSend /begynn for å starte spillet.");
            }
            #endif

            var text = message.Text.ToLower();
            if (ApprovalState.ContainsKey(fromId))
            {
                // Approval status
                var player = ApprovalState[fromId];

                if (text == "/neste")
                {
                    playersWaitingForApproval.Enqueue(player);
                    GetNextForApproval(fromId);
                    return;
                }

                if (text == "/godkjenn")
                {
                    player.State = PlayerState.WaitingForGameStart;
                    Task.Factory.StartNew(async () =>
                    {
                        _sender.SendMessage(player, string.Format(Messages.ApplicationApproved, player.AgentName));
                        await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(true);
                        _sender.SendMessage(player, Messages.GameRulesEliminate);
                        await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(true);
                        _sender.SendMessage(player, Messages.GameRulesReveal);
                    });
                    

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
                    case "/status" when _game.State < GameState.Started:
                        int waitingPlayers = _playerRepository.GetWaitingPlayerCount();
                        _sender.SendMessage(fromId, $"Det er {waitingPlayers} agenter som venter på start.\n{playersWaitingForApproval.Count} agenter som venter på godkjenning.");
                        return;
                    case "/neste":
                        GetNextForApproval(fromId);
                        return;
                    case "/begynn" when playersWaitingForApproval.Count != 0:
                        _sender.SendMessage(fromId, $"Det er fremdeles {playersWaitingForApproval.Count} agenter som må godkjennes");
                        return;
                    case "/begynn":
                        int waitingPlayers2 = _playerRepository.GetWaitingPlayerCount();
                        if (waitingPlayers2 > 3)
                        {
                            _sender.SendMessage(fromId, $"Det er {waitingPlayers2} agenter som venter på start.\nEr du sikker på at du vil starte spillet?\n/Ja - Starter spillet\n/Nei - utsett start");
                            _game.State = GameState.PreStart;
                        }
                        else
                        {
                            _sender.SendMessage(fromId, "Det er ikke nok spillere til å starte spillet. Det må være minst 3 spillere.");
                        }
                        return;
                    case "/ja" when _game.State == GameState.PreStart:
                        _sender.SendMessage(fromId, $"Spillet er startet!");
                        _game.StartGame();
                        return;
                    case "/nei" when _game.State == GameState.PreStart:
                        _game.State = GameState.NotStarted;
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