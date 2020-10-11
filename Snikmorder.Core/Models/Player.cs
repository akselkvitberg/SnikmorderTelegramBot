using System;

namespace Snikmorder.Core.Models
{
    public class Player
    {
        public Player()
        {
            
        }

        public Player(int telegramUserId)
        {
            TelegramUserId = telegramUserId;
        }
        public int TelegramUserId { get; set; }
        public string? PlayerName { get; set; }
        public string? AgentName { get; set; }
        public string? PictureId { get; set; }

        public bool IsActive => State >= PlayerState.Active && State < PlayerState.Killed;
        
        public PlayerState State { get; set; }
        
        public int TargetId { get; set; }
        public int? ApprovalId { get; set; }
    }
}