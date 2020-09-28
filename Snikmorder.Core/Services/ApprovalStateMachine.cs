using System.Collections.Generic;
using Snikmorder.Core.Models;
using Snikmorder.Core.Resources;
using Telegram.Bot.Types;

namespace Snikmorder.Core.Services
{
    public class ApprovalStateMachine
    {
        private readonly ITelegramSender _sender;
        Queue<Player> playersWaitingForApproval = new Queue<Player>();

        Dictionary<int, Player> ApprovalState = new Dictionary<int, Player>();

        public ApprovalStateMachine(ITelegramSender sender)
        {
            _sender = sender;
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
                    _sender.SendMessage(fromId, "Forsto ikke meldingen. Send /godkjenn eller /forkast");
                }
            }
            else
            {
                if (text == "/neste")
                {
                    GetNextForApproval(fromId);
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
                _sender.SendMessage(fromId, "Ingen agenter til godkjenning.");
            }
        }

        public void AddApplication(Player player)
        {
            playersWaitingForApproval.Enqueue(player);
        }
    }
}