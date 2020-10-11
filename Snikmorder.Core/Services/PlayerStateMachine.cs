using System;
using System.Linq;
using System.Threading.Tasks;
using Snikmorder.Core.Models;
using Snikmorder.Core.Resources;
using Telegram.Bot.Types;
using Game = Snikmorder.Core.Models.Game;

namespace Snikmorder.Core.Services
{
    public class PlayerStateMachine
    {
        private readonly ITelegramSender _sender;
        private readonly IPlayerRepository _playerRepository;
        private readonly AdminStateMachine _adminStateMachine;
        private readonly GameService _gameService;

        public PlayerStateMachine(ITelegramSender sender, IPlayerRepository playerRepository, AdminStateMachine adminStateMachine, GameService gameService)
        {
            _sender = sender;
            _playerRepository = playerRepository;
            _adminStateMachine = adminStateMachine;
            _gameService = gameService;
        }

        public async Task HandlePlayerMessage(Message message)
        {
            // Get Player by ID
            Player? player = await _playerRepository.GetPlayer(message.From.Id);

            var game = await _gameService.GetGame();

            if (player == null)
            {
                player = HandleNewPlayer(message);
            }

            if (game.State == GameState.Ended)
            {
                await _sender.SendMessage(player, "Spillet er over.");
                return;
            }
            
            if (game.State >= GameState.Started && player.State < PlayerState.Active)
            {
                await _sender.SendMessage(player, "Spillet er allerede i gang. Du rakk desverre ikke å bli med.");
                return;
            }

            if (player.State < PlayerState.WaitingForAdminApproval && string.Equals(message.Text, "/nysøknad", StringComparison.InvariantCultureIgnoreCase))
            {
                player.State = PlayerState.Started;
            }

            if (player.IsActive)
            {
                if(await HandleGenericActiveState(player, message))
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
                    await HandleActiveState(player, message);
                    break;
                case PlayerState.ConfirmKill:
                    await HandleConfirmKill(player, message);
                    break;
                case PlayerState.ReportingKilling:
                    await HandleReportingKilling(player, message);
                    break;
                case PlayerState.Killed:
                    HandleKilled(player, message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            await _playerRepository.Save();
        }

        private Player HandleNewPlayer(Message message)
        {
            var p = new Player(message.From.Id);

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

                if (text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 1)
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

        private async Task<bool> HandleGenericActiveState(Player player, Message message)
        {
            var text = message.Text.ToLower();

            switch (text)
            {
                case "/hjelp":
                    await _sender.SendMessage(player, Messages.PlayerHelp);
                    return true;
                case "/regler":
                    await _sender.SendMessage(player, Messages.GameRulesEliminate);
                    await _sender.SendMessage(player, Messages.GameRulesReveal);
                    return true;
                case "/info":
                    var target = await _playerRepository.GetPlayer(player.TargetId);
                    await _sender.SendImage(player, string.Format(Messages.PlayerInfo, player.AgentName, target?.PlayerName), target?.PictureId);
                    return true;
                default:
                    return false;
            }
        }

        private async Task HandleActiveState(Player player, Message message)
        {
            if (TextIsEmpty(player, message)) return;

            var text = message.Text.ToLower();

            var strings = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            

            if(strings[0] == "/eliminer") // eliminer er litt vanskelig å skrive... bedre ord?
            {
                if (strings.Length > 1)
                {
                    message.Text = string.Join(" ", strings[1..]);
                    await HandleConfirmKill(player, message);
                }
                else
                {
                    await _sender.SendMessage(player, "Bekreft elimineringen ved å skrive inn det hemmelige agent-navnet til målet ditt.");
                    player.State = PlayerState.ConfirmKill;
                }
            }
            else if (strings[0] == ("/avslør"))
            {
                if (strings.Length > 1)
                {
                    message.Text = string.Join(" ", strings[1..]);
                    await HandleReportingKilling(player, message);
                }
                else
                {
                    await _sender.SendMessage(player, "Bekreft avsløringen ved å skrive inn det hemmelige agent-navnet til agenten.");
                    player.State = PlayerState.ReportingKilling;
                }
            }
        }

        private async Task HandleConfirmKill(Player player, Message message)
        {
            if (TextIsEmpty(player, message)) return;

            if (message.Text.ToLower() == "/avbryt")
            {
                await _sender.SendMessage(player, "Elimineringen er avbrutt");
                player.State = PlayerState.Active;
                return;
            }

            var target = await _playerRepository.GetPlayer(player.TargetId);
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
                await _sender.SendMessage(player, "Alle agentnavn består bare av et ord. Prøv igjen.");
                return;
            }

            targetAgentName = targetAgentName.ToLower().Replace("agent", "").Trim();

            if (agentName != targetAgentName)
            {
                player.State = PlayerState.Active;
                await _sender.SendMessage(player, "Dette var ikke riktig agent-navn. Du har enten feil mål eller feil agent-navn.\nKontakt HQ hvis du mener dette er feil.");
                return;
            }

            player.State = PlayerState.Active;
            target.State = PlayerState.Killed; // todo: Save
            await _sender.SendMessage(target, "Beklager, du er ute av spillet.");
            await _sender.SendMessage(player, $"Agent {target.AgentName} er bekreftet eliminert!");

            var newTarget = await _playerRepository.GetPlayer(target.TargetId);

            if (newTarget.TargetId == player.TelegramUserId)
            {
                // Win state
                await _gameService.EndWithWinners(player, newTarget);
                return;
            }

            player.TargetId = target.TargetId;

            if (newTarget != null)
            {
                await _sender.SendImage(player, string.Format(Messages.NextTarget, newTarget.PlayerName), newTarget.PictureId);
            }
        }

        private async Task HandleReportingKilling(Player player, Message message)
        {
            if (TextIsEmpty(player, message)) return;

            if (message.Text.ToLower() == "/avbryt")
            {
                await _sender.SendMessage(player, "Avsløring avbrutt.");
                player.State = PlayerState.Active;
                return;
            }

            var agentName = message.Text.ToLower();

            agentName = agentName.Replace("agent", "").Trim();

            var agent = await _playerRepository.GetPlayerByAgentName(agentName);
            
            if (agent == null)
            {
                player.State = PlayerState.Active;
                await _sender.SendMessage(player, "Det finnes ingen agenter med dette navnet. Avsløringen er avbrutt.");
                return;
            }
            if (!agent.IsActive)
            {
                player.State = PlayerState.Active;
                await _sender.SendMessage(player, $"Agent {agent.AgentName} ({agent.PlayerName}) er allerede ute.");
                return;
            }

            // Cannot reveal hunter
            if (agent.TargetId == player.TelegramUserId)
            {
                player.State = PlayerState.Active;
                await _sender.SendMessage(player, $"Du kan ikke avsløre Agent {agent.AgentName}!");
                return;
            }

            if (agent.TelegramUserId == player.TelegramUserId)
            {
                player.State = PlayerState.Active;
                await _sender.SendMessage(player, $"Du kan ikke avsløre deg selv!");
                return;
            }

            

            agent.State = PlayerState.Killed; //todo save
            
            var newTarget = await _playerRepository.GetPlayer(agent.TargetId);
            var hunter = await _playerRepository.GetHunter(agent.TelegramUserId);

            if (hunter == null || newTarget == null)
            {
                //todo what?
                throw new NullReferenceException();
            }

            hunter.TargetId = newTarget.TelegramUserId;

            player.State = PlayerState.Active;
            
            if ((await _playerRepository.GetAllPlayersActive()).Count == 2)
            {
                // Win state
                await _gameService.EndWithWinners(player, newTarget);
                return;
            }

            await _sender.SendMessage(player, $"Agent {agent.AgentName} ble avslørt og er ute av spllet!");
            await _sender.SendMessage(agent, "Beklager, du ble avslørt og er ute av spillet.");
            await _sender.SendImage(hunter, string.Format(Messages.NextTarget, newTarget.PlayerName), newTarget.PictureId);
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