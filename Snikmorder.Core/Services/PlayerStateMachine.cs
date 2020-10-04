using System;
using System.Linq;
using System.Net.Mime;
using Snikmorder.Core.Models;
using Snikmorder.Core.Resources;
using Telegram.Bot.Types;
using Game = Snikmorder.Core.Models.Game;

namespace Snikmorder.Core.Services
{
    public class PlayerStateMachine
    {
        private readonly ITelegramSender _sender;
        private readonly PlayerRepository _playerRepository;
        private readonly ApprovalStateMachine _approvalStateMachine;
        private readonly Game _game;

        public PlayerStateMachine(ITelegramSender sender, PlayerRepository playerRepository, ApprovalStateMachine approvalStateMachine, Game game)
        {
            _sender = sender;
            _playerRepository = playerRepository;
            _approvalStateMachine = approvalStateMachine;
            _game = game;
        }

        public void HandlePlayerMessage(Message message)
        {
            // Get Player by ID
            Player? player = _playerRepository.GetPlayer(message.From.Id);

            if (player == null)
            {
                player = HandleNewPlayer(message);
            }


            if (_game.IsStarted && player.State < PlayerState.Active)
            {
                _sender.SendMessage(player, "Spillet er allerede i gang. Du rakk desverre ikke å bli med.");
                return;
            }


            if (player.State < PlayerState.WaitingForAdminApproval && string.Equals(message.Text, "/nysøknad", StringComparison.InvariantCultureIgnoreCase))
            {
                player.State = PlayerState.Started;
            }

            if (player.IsActive)
            {
                if(HandleGenericActiveState(player, message))
                    return;
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
                    HandleWaitingForAdminApproval(player, message);
                    break;
                case PlayerState.WaitingForGameStart:
                    HandleWaitingForGameStart(player, message);
                    break;
                case PlayerState.Active:
                    HandleActiveState(player, message);
                    break;
                case PlayerState.ConfirmKill:
                    HandleConfirmKill(player, message);
                    break;
                case PlayerState.ReportingKilling:
                    HandleReportingKilling(player, message);
                    break;
                case PlayerState.Killed:
                    HandleKilled(player, message);
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
                TelegramUserId = message.From.Id,
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
            player.AgentName = agentName;
            player.State = PlayerState.GivingAgentName;
        }

        private void HandleGivingAgentName(Player player, Message message)
        {
            if (TextIsEmpty(player, message)) return;

            var text = message.Text.ToLower().Trim();

            // If player sends /ok, keep temporary agent name
            if(text != "/ok")
            {
                if (text.Contains("agent"))
                {
                    text = text.Replace("agent", "").Trim();
                }

                if (text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 1)
                {
                    _sender.SendMessage(player, $"Agentnavn kan ikke ha mellomrom. Prøv igjen, eller send /ok for å bruke {player.AgentName} som ditt agentnavn");
                    return;
                }

                var capitalized = text[0].ToString().ToUpper() + text[1..];

                player.AgentName = capitalized;
            }
            player.State = PlayerState.GivingSelfie;
            var requestSelfie = string.Format(Messages.RequestSelfie, player.AgentName);
            _sender.SendMessage(player, requestSelfie);
        }

        private void HandleGivingSelfie(Player player, Message message)
        {
            if (message.Photo == null || message.Photo.Length == 0)
            {
                _sender.SendMessage(player, Messages.UnknownResponse);
                return;
            }

            
            player.PictureId = message.Photo.OrderByDescending(x => x.Height).FirstOrDefault()?.FileId;
            player.State = PlayerState.ConfirmApplication;

            var confirmApplication = string.Format(Messages.ConfirmApplication, player.PlayerName, player.AgentName);
            _sender.SendMessage(player, confirmApplication);

        }

        private void HandleConfirmApplication(Player player, Message message)
        {
            if (TextIsEmpty(player, message)) return;

            // /nySøknad is handled at a higher level
            if (!string.Equals(message.Text, "/ok", StringComparison.InvariantCultureIgnoreCase))
            {
                _sender.SendMessage(player, Messages.UnknownResponse);
                return;
            }

            _approvalStateMachine.AddApplication(player);

            _sender.SendMessage(player, Messages.ApplicationRegistered);
            player.State = PlayerState.WaitingForAdminApproval;
        }

        private void HandleWaitingForAdminApproval(Player player, Message message)
        {
            _sender.SendMessage(player, Messages.WaitForAdminApproval);
        }

        private void HandleWaitingForGameStart(Player player, Message message)
        {
            _sender.SendMessage(player, Messages.WaitForGameStart);
        }

        private bool HandleGenericActiveState(Player player, Message message)
        {
            var text = message.Text.ToLower();

            switch (text)
            {
                case "/hjelp":
                    _sender.SendMessage(player, Messages.PlayerHelp);
                    return true;
                case "/regler":
                    _sender.SendMessage(player, Messages.GameRulesEliminate);
                    _sender.SendMessage(player, Messages.GameRulesReveal);
                    return true;
                case "/info":
                    var target = _playerRepository.GetPlayer(player.TargetId);
                    _sender.SendImage(player, string.Format(Messages.PlayerInfo, player.AgentName, target?.PlayerName), target?.PictureId);
                    return true;
                default:
                    return false;
            }
        }

        private void HandleActiveState(Player player, Message message)
        {
            if (TextIsEmpty(player, message)) return;

            if(message.Text.ToLower().StartsWith("/eliminer")) // eliminer er litt vanskelig å skrive... bedre ord?
            {
                _sender.SendMessage(player, "Bekreft elimineringen ved å skrive inn det hemmelige agent-navnet til målet ditt.");
                player.State = PlayerState.ConfirmKill;
            }
            else if (message.Text.ToLower().StartsWith("/avslør"))
            {
                _sender.SendMessage(player, "Bekreft avsløringen ved å skrive inn det hemmelige agent-navnet til agenten.");
                player.State = PlayerState.ReportingKilling;
            }
        }

        private void HandleConfirmKill(Player player, Message message)
        {
            if (TextIsEmpty(player, message)) return;

            if (message.Text.ToLower() == "/avbryt")
            {
                _sender.SendMessage(player, "Elimineringen er avbrutt");
                player.State = PlayerState.Active;
                return;
            }

            var target = _playerRepository.GetPlayer(player.TargetId);
            if (target == null)
            {
                throw new NullReferenceException();
                return;// todo: error
            }
            
            var targetAgentName = target.AgentName;

            if (targetAgentName == null)
            {
                // something bad happened - no agent name set on agent
                return;
                // log this
            }

            var agentName = message.Text.ToLower().Replace("agent", "").Trim();

            // No spaces
            if (agentName.Contains(' '))
            {
                _sender.SendMessage(player, "Alle agentnavn består bare av et ord. Prøv igjen.");
                return;
            }

            targetAgentName = targetAgentName.ToLower().Replace("agent", "").Trim();

            if (agentName != targetAgentName)
            {
                player.State = PlayerState.Active;
                _sender.SendMessage(player, "Dette var ikke riktig agent-navn. Du har enten feil mål eller feil agent-navn.\nKontakt HQ hvis du mener dette er feil.");
                return;
            }

            player.State = PlayerState.Active;
            target.State = PlayerState.Killed; // todo: Save
            var newTarget = _playerRepository.GetPlayer(target.TargetId);
            player.TargetId = target.TargetId;

            _sender.SendMessage(target, "Beklager, du er ute av spillet.");
            if (newTarget != null)
            {
                _sender.SendImage(player, string.Format(Messages.NextTarget, newTarget.PlayerName), newTarget.PictureId);
            }
        }

        private void HandleReportingKilling(Player player, Message message)
        {
            if (TextIsEmpty(player, message)) return;

            if (message.Text.ToLower() == "/avbryt")
            {
                _sender.SendMessage(player, "Avsløring avbrutt.");
                player.State = PlayerState.Active;
                return;
            }

            var agentName = message.Text.ToLower();

            agentName = agentName.Replace("agent", "").Trim();

            var agent = _playerRepository.GetPlayerByAgentName(agentName);

            if (agent == null)
            {
                player.State = PlayerState.Active;
                _sender.SendMessage(player, "Det finnes ingen agenter med dette navnet. Avsløringen er avbrutt.");
                return;
            }

            agent.State = PlayerState.Killed; //todo save
            var newTarget = _playerRepository.GetPlayer(agent.TargetId);
            var hunter = _playerRepository.GetHunter(agent.TelegramChatId);

            if (hunter == null || newTarget == null)
            {
                //todo what?
                throw new NullReferenceException();
            }

            hunter.TargetId = newTarget.TargetId;

            player.State = PlayerState.Active;

            _sender.SendMessage(player, $"Agent {agent.AgentName} ble avslørt og er ute av spllet!");
            _sender.SendMessage(agent, "Beklager, du ble avslørt og er ute av spillet.");
            _sender.SendImage(hunter, string.Format(Messages.NextTarget, newTarget.PlayerName), newTarget.PictureId);
        }

        private void HandleKilled(Player player, Message message)
        {
            _sender.SendMessage(player, "Beklager, du er død, og døde agenter kan ikke gjøre noe.\nKontakt HQ hvis du mener dette er feil.");
        }

        private bool TextIsEmpty(Player player, Message message)
        {
            if (!string.IsNullOrWhiteSpace(message?.Text)) return false;
            _sender.SendMessage(player, Messages.UnknownResponse);
            return true;
        }
    }
}