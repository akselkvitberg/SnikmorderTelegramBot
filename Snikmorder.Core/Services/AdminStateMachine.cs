using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Snikmorder.Core.Models;
using Snikmorder.Core.Resources;
using Telegram.Bot.Types;

namespace Snikmorder.Core.Services
{
    public class AdminStateMachine
    {
        private readonly ITelegramSender _sender;
        private readonly IPlayerRepository _playerRepository;
        private readonly GameService _gameService;

        public AdminStateMachine(ITelegramSender sender, IPlayerRepository playerRepository, GameService gameService)
        {
            _sender = sender;
            _playerRepository = playerRepository;
            _gameService = gameService;
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

        public async Task HandleAdminMessage(Message message)
        {
            var fromId = message.From.Id;

            var game = await _gameService.GetGame();
            

            if (game.State == GameState.Started)
            {
                switch (message.Text.ToLower())
                {
                    case "/hjelp":
                        await _sender.SendMessage(fromId, "Kommandoer:\n/status - se hvor mange agenter som er i spill\n/oppdrag - se hvilke mål hver agent har");
                        return;
                    case "/status":
                        int activePlayers = (await _playerRepository.GetAllPlayersActive()).Count;
                        int deadPlayers = (await _playerRepository.GetAllPlayersInState(PlayerState.Killed)).Count;
                        await _sender.SendMessage(fromId, $"Det er {activePlayers} agenter i spill.\nDet er {deadPlayers} døde agenter.");
                        return;
                    case "/oppdrag":
                        return;
                }
                return;
            }

            if (game.State == GameState.Ended)
            {
                if (string.Equals(message.Text, "/restart", StringComparison.OrdinalIgnoreCase))
                {
                    await _gameService.SetState(GameState.NotStarted);
                    await _playerRepository.Reset();
                }
                return;
            }

            
            // Handle messages such as "approve application"
            #if DEBUG
            if (message.Text == "/all")
            {
                var playersWaitingForApproval = await _playerRepository.GetAllPlayersInState(PlayerState.WaitingForAdminApproval);
                foreach (var player in playersWaitingForApproval)
                {
                    player.State = PlayerState.WaitingForGameStart;
                    player.ApprovalId = null;
                    await _sender.SendMessage(player, string.Format(Messages.ApplicationApproved, player.AgentName));
                }
                await _playerRepository.Save();
                playersWaitingForApproval.Clear();
                await _sender.SendMessage(fromId, "Det er ingen agenter til godkjenning.\nSend /begynn for å starte spillet.");
            }
            #endif

            var text = message.Text.ToLower();

            var playerApprovedBy = await _playerRepository.GetPlayerApprovedBy(fromId);

            if (playerApprovedBy != null)
            {
                // Approval status

                if (text == "/neste")
                {
                    await _sender.SendMessage(fromId, "Send enten /godkjenn eller /forkast");
                    return;
                }

                if (text == "/godkjenn")
                {
                    playerApprovedBy.State = PlayerState.WaitingForGameStart;
                    playerApprovedBy.ApprovalId = null;
                    await _playerRepository.Save();
                    _ = Task.Factory.StartNew(async () =>
                    {
                        await _sender.SendMessage(playerApprovedBy, string.Format(Messages.ApplicationApproved, playerApprovedBy.AgentName));
                        await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(true);
                        await _sender.SendMessage(playerApprovedBy, Messages.GameRulesEliminate);
                        await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(true);
                        await _sender.SendMessage(playerApprovedBy, Messages.GameRulesReveal);
                    });
                    

                    await GetNextForApproval(fromId);
                }
                else if (text == "/forkast")
                {
                    playerApprovedBy.State = PlayerState.Started;
                    await _playerRepository.Save();
                    await _sender.SendMessage(playerApprovedBy, Messages.ApplicationNotApproved);
                    await GetNextForApproval(fromId);
                }
                else
                {
                    await _sender.SendMessage(fromId, Messages.UnknownApprovalMessage);
                }
            }
            else
            {
                switch (text)
                {
                    case "/hjelp":
                        await _sender.SendMessage(fromId, Messages.ApprovalHelp);
                        return;
                    case "/status" when game.State < GameState.Started:
                    {
                        //int waitingPlayers = await _playerRepository.GetWaitingPlayerCount();
                        //int approvalCount =  await _playerRepository.CountAllPlayersInState(PlayerState.WaitingForAdminApproval);
                        //approvalCount += await _playerRepository.CountAllPlayersInState(PlayerState.PickedForAdminApproval);
                        //await _sender.SendMessage(fromId, $"Det er {waitingPlayers} agenter som venter på start.\n{approvalCount} agenter som venter på godkjenning.");
                        return;
                    }
                    case "/neste":
                        await GetNextForApproval(fromId);
                        return;
                    case "/begynn":
                    {
                        int approvalCount = (await _playerRepository.GetAllPlayersInState(PlayerState.WaitingForAdminApproval)).Count;
                        approvalCount += (await _playerRepository.GetAllPlayersInState(PlayerState.PickedForAdminApproval)).Count;
                        if (approvalCount > 0)
                        {
                            await _sender.SendMessage(fromId, $"Det er fremdeles {approvalCount} agenter som må godkjennes");
                            return;
                        }

                        int waitingPlayers2 = (await _playerRepository.GetAllPlayersInState(PlayerState.WaitingForGameStart)).Count;
                        if (waitingPlayers2 > 3)
                        {
                            await _sender.SendMessage(fromId, $"Det er {waitingPlayers2} agenter som venter på start.\nEr du sikker på at du vil starte spillet?\n/Ja - Starter spillet\n/Nei - utsett start");
                            await _gameService.SetState(GameState.PreStart);
                        }
                        else
                        {
                            await _sender.SendMessage(fromId, "Det er ikke nok spillere til å starte spillet. Det må være minst 3 spillere.");
                        }

                        return;
                    }
                    case "/ja" when game.State == GameState.PreStart:
                        await _sender.SendMessage(fromId, $"Spillet er startet!");
                        await _gameService.StartGame();
                        return;
                    case "/nei" when game.State == GameState.PreStart:
                        await _gameService.SetState(GameState.NotStarted);
                        return;
                }
            }
        }

        private async Task GetNextForApproval(int fromId)
        {
            var waitingForApproval = await _playerRepository.GetAllPlayersInState(PlayerState.WaitingForAdminApproval);
            var player = waitingForApproval.FirstOrDefault();
            if (player != null)
            {
                player.ApprovalId = fromId;
                player.State = PlayerState.PickedForAdminApproval;
                await _playerRepository.Save();
                await _sender.SendImage(fromId, $"Navn: {player.PlayerName}\nAgent {player.AgentName}\n/godkjenn eller \n/forkast", player.PictureId);
            }
            else
            {
                await _sender.SendMessage(fromId, "Det er ingen agenter til godkjenning.\nSend /begynn for å starte spillet.");
            }
        }
    }
}