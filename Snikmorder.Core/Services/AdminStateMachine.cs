using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Snikmorder.Core.Models;
using Snikmorder.Core.Resources;
using Telegram.Bot.Types;

namespace Snikmorder.Core.Services
{
    public class AdminStateMachine
    {
        private readonly ITelegramSender _sender;
        private readonly IGameRepository _gameRepository;
        private readonly GameService _gameService;

        public AdminStateMachine(ITelegramSender sender, IGameRepository gameRepository, GameService gameService)
        {
            _sender = sender;
            _gameRepository = gameRepository;
            _gameService = gameService;
        }

        public async Task<bool> IsFromAdmin(Message message)
        {
            if (message.From.Id == 0)
            {
                return true;
            }

            if (await _gameRepository.IsAdmin(message.From.Id))
            {
                return true;
            }
            return false;
        }

        public async Task HandleAdminMessage(Message message)
        {
            var fromId = message.From.Id;

            if (message.Contact != null)
            {
                await _gameRepository.AddAdmin(message.Contact);
                return;
            }

            if (string.Equals(message.Text, "/admins", StringComparison.OrdinalIgnoreCase))
            {
                var admins = await _gameRepository.GetAdmins();
                var adminList = string.Join("\n", admins.Select(x => $"{x.FirstName} {x.LastName}"));
                await _sender.SendMessage(fromId, "Admins:" + adminList);
                return;
            }

            var gameState = await _gameRepository.GetGameState();
            

            if (gameState == GameState.Started)
            {
                switch (message.Text.ToLower())
                {
                    case "/hjelp":
                        await _sender.SendMessage(fromId, "Kommandoer:\n/status - se hvor mange agenter som er i spill\n/oppdrag - se hvilke mål hver agent har");
                        return;
                    case "/status":
                        int activePlayers = (await _gameRepository.GetAllPlayersActive()).Count;
                        int deadPlayers = (await _gameRepository.GetAllPlayersInState(PlayerState.Killed)).Count;
                        await _sender.SendMessage(fromId, $"Det er {activePlayers} agenter i spill.\nDet er {deadPlayers} døde agenter.");
                        return;
                    case "/oppdrag":
                        var allPlayersActive = await _gameRepository.GetAllPlayersActive();
                        var dict = allPlayersActive.ToDictionary(x => x.TelegramUserId);

                        StringBuilder sb = new StringBuilder();
                        var player = allPlayersActive.First();
                        sb.AppendLine($"{player.PlayerName} - Agent {player.AgentName}");

                        while (dict.Count > 0)
                        {
                            player = dict[player.TargetId];
                            dict.Remove(player.TelegramUserId);

                            sb.AppendLine("⬇");
                            sb.AppendLine($"{player.PlayerName} - Agent {player.AgentName}");
                        }

                        await _sender.SendMessage(fromId, sb.ToString());
                        return;
                }
                return;
            }

            if (gameState == GameState.Ended)
            {
                if (string.Equals(message.Text, "/restart", StringComparison.OrdinalIgnoreCase))
                {
                    await _gameRepository.SetGameState(GameState.NotStarted);
                    await _gameRepository.Reset();
                }
                return;
            }

            
            // Handle messages such as "approve application"
            #if DEBUG
            if (message.Text == "/all")
            {
                var playersWaitingForApproval = await _gameRepository.GetAllPlayersInState(PlayerState.WaitingForAdminApproval);
                foreach (var player in playersWaitingForApproval)
                {
                    player.State = PlayerState.WaitingForGameStart;
                    player.ApprovalId = null;
                    await _sender.SendMessage(player, string.Format(Messages.ApplicationApproved, player.AgentName));
                }
                await _gameRepository.Save();
                playersWaitingForApproval.Clear();
                await _sender.SendMessage(fromId, "Det er ingen agenter til godkjenning.\nSend /begynn for å starte spillet.");
            }
            #endif

            var text = message.Text.ToLower();

            var playerApprovedBy = await _gameRepository.GetPlayerApprovedBy(fromId);

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
                    await _gameRepository.Save();
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
                    playerApprovedBy.ApprovalId = null;
                    await _gameRepository.Save();
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
                    
                    case "/status" when gameState < GameState.Started:
                    {
                            var waitingPlayers = await _gameRepository.GetAllPlayersInState(PlayerState.WaitingForGameStart);
                            var approvalCount = (await _gameRepository.GetAllPlayersInState(PlayerState.WaitingForAdminApproval)).Count;
                            approvalCount += (await _gameRepository.GetAllPlayersInState(PlayerState.PickedForAdminApproval)).Count;
                            await _sender.SendMessage(fromId, $"Det er {waitingPlayers.Count} agenter som venter på start.\n{approvalCount} agenter som venter på godkjenning.");
                            return;
                    }
                    case "/neste":
                        await GetNextForApproval(fromId);
                        return;
                    case "/begynn":
                    {
                        int approvalCount = (await _gameRepository.GetAllPlayersInState(PlayerState.WaitingForAdminApproval)).Count;
                        approvalCount += (await _gameRepository.GetAllPlayersInState(PlayerState.PickedForAdminApproval)).Count;
                        if (approvalCount > 0)
                        {
                            await _sender.SendMessage(fromId, $"Det er fremdeles {approvalCount} agenter som må godkjennes");
                            return;
                        }

                        int waitingPlayers2 = (await _gameRepository.GetAllPlayersInState(PlayerState.WaitingForGameStart)).Count;
                        if (waitingPlayers2 > 3)
                        {
                            await _sender.SendMessage(fromId, $"Det er {waitingPlayers2} agenter som venter på start.\nEr du sikker på at du vil starte spillet?\n/Ja - Starter spillet\n/Nei - utsett start");
                            await _gameRepository.SetGameState(GameState.PreStart);
                        }
                        else
                        {
                            await _sender.SendMessage(fromId, "Det er ikke nok spillere til å starte spillet. Det må være minst 3 spillere.");
                        }

                        return;
                    }
                    case "/ja" when gameState == GameState.PreStart:
                        await _sender.SendMessage(fromId, $"Spillet er startet!");
                        await _gameService.StartGame();
                        return;
                    case "/nei" when gameState == GameState.PreStart:
                        await _gameRepository.SetGameState(GameState.NotStarted);
                        return;
                    default:
                        await _sender.SendMessage(fromId, Messages.ApprovalHelp);
                        return;
                }
            }
        }

        private async Task GetNextForApproval(int fromId)
        {
            var waitingForApproval = await _gameRepository.GetAllPlayersInState(PlayerState.WaitingForAdminApproval);
            var player = waitingForApproval.FirstOrDefault();
            if (player != null)
            {
                player.ApprovalId = fromId;
                player.State = PlayerState.PickedForAdminApproval;
                await _gameRepository.Save();
                await _sender.SendImage(fromId, $"Navn: {player.PlayerName}\nAgent {player.AgentName}\n/godkjenn eller \n/forkast", player.PictureId);
            }
            else
            {
                await _sender.SendMessage(fromId, "Det er ingen agenter til godkjenning.\nSend /begynn for å starte spillet.");
            }
        }
    }
}