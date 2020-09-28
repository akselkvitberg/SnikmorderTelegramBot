using System;
using System.Linq;
using SnikmorderTelegramBot.Models;
using SnikmorderTelegramBot.Resources;
using Telegram.Bot.Types;

namespace SnikmorderTelegramBot.Services
{
    public class PlayerStateMachine
    {
        private readonly TelegramSender _sender;
        private readonly PlayerRepository _playerRepository;

        public PlayerStateMachine(TelegramSender sender, PlayerRepository playerRepository)
        {
            _sender = sender;
            _playerRepository = playerRepository;
        }

        public void HandlePlayerMessage(Message message)
        {
            // Get Player by ID
            var player = _playerRepository.GetPlayer(message.From.Id);

            if (player == null)
            {
                player = HandleNewPlayer(message);
            }

            if (player.State < PlayerState.WaitingForAdminApproval && message.Text.ToLowerInvariant() == "/nysøknad")
            {
                player.State = PlayerState.Started;
            }

            switch (player.State)
            {
                case PlayerState.Started:
                    HandleStarted(player, message);
                    break;
                case PlayerState.GivingName:
                    HandleGivingName(player, message);
                    break;
                case PlayerState.GivingAgentName:
                    HandleGivingAgentName(player, message);
                    break;
                case PlayerState.GivingSelfie:
                    HandleGivingSelfie(player, message);
                    break;
                case PlayerState.ConfirmApplication:
                    HandleConfirmApplication(player, message);
                    break;
                case PlayerState.WaitingForAdminApproval:
                    break;
                case PlayerState.WaitingForGameStart:
                    break;
                case PlayerState.Active:
                    break;
                case PlayerState.Killing:
                    break;
                case PlayerState.WaitingForNewTarget:
                    break;
                case PlayerState.ReportingKilling:
                    break;
                case PlayerState.Killed:
                    break;
                case PlayerState.Dead:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _playerRepository.Save(player);
        }

        private Player HandleNewPlayer(Message message)
        {
            var p = new Player()
            {
                State = PlayerState.Started,
                TelegramUserId = message.MessageId,
                TelegramChatId = message.Chat.Id,
            };

            _playerRepository.AddPlayer(p);
            return p;
        }

        private void HandleStarted(Player player, Message message)
        {
            _sender.SendMessage(player, Messages.WelcomeMessage);
            player.State = PlayerState.GivingName;
        }

        private void HandleGivingName(Player player, Message message)
        {
            if (TextIsEmpty(player, message)) return;

            var agentName = AgentNameGenerator.GetAgentName();
            var requestAgentName = string.Format(Messages.RequestAgentName, agentName);
            _sender.SendMessage(player, requestAgentName);
            player.PlayerName = message.Text;
            // Store temporary name
            player.AgentName = agentName;
            player.State = PlayerState.GivingAgentName;
        }

        private void HandleGivingAgentName(Player player, Message message)
        {
            if (TextIsEmpty(player, message)) return;

            // If player sends /ok, keep temporary agent name
            if (message.Text.ToLowerInvariant() != "/ok")
            {
                player.AgentName = message.Text;
            }
            player.State = PlayerState.GivingSelfie;
            _sender.SendMessage(player, Messages.RequestSelfie);
        }

        private void HandleGivingSelfie(Player player, Message message)
        {
            if (TextIsEmpty(player, message)) return;

            _sender.SendMessage(player, Messages.ApplicationRegistered);
            player.PictureId = message.Photo.OrderByDescending(x=>x.Height).FirstOrDefault()?.FileId;
            player.State = PlayerState.ConfirmApplication;
        }

        private void HandleConfirmApplication(Player player, Message message)
        {
            if (TextIsEmpty(player, message)) return;

            
            // /nySøknad is handled at a higher level
            if (message.Text.ToLowerInvariant() != "/ok")
            {
                _sender.SendMessage(player, Messages.UnknownResponse);
                return;
            }

            _sender.SendMessage(player, Messages.ApplicationRegistered);
            player.State = PlayerState.WaitingForAdminApproval;
        }

        private bool TextIsEmpty(Player player, Message message)
        {
            if (string.IsNullOrWhiteSpace(message.Text))
            {
                _sender.SendMessage(player, Messages.UnknownResponse);
                return true;
            }

            return false;
        }
    }

    internal static class AgentNameGenerator
    {
        public static string GetAgentName()
        {
            return "Fiskeben";
        }
    }

    public class PlayerRepository
    {
        public Player GetPlayer(int telegramId)
        {
            return new Player()
            {
                
            };
        }

        public void AddPlayer(Player player)
        {
            throw new NotImplementedException();
        }

        public void Save(Player player)
        {
            throw new NotImplementedException();
        }
    }
}