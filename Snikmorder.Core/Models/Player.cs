using System;

namespace Snikmorder.Core.Models
{
    public class Player
    {
        public Player()
        {
            
        }

        public Player(long telegramUserId)
        {
            TelegramUserId = telegramUserId;
        }
        public long TelegramUserId { get; set; }
        public string? PlayerName { get; set; }
        public string? AgentName { get; set; }
        public string? PictureId { get; set; }

        public bool IsActive => State >= PlayerState.Active && State < PlayerState.Killed;
        
        public PlayerState State { get; set; }
        
        public long TargetId { get; set; }
        public long? ApprovalId { get; set; }
    }
}