namespace SnikmorderTelegramBot.Models
{
    public class Player
    {
        public int TelegramUserId { get; set; }
        public string PlayerName { get; set; }
        public string AgentName { get; set; }
        public string PictureId { get; set; }
        public PlayerState State { get; set; }
        
        public Player Target { get; set; }
        public Player Hunter { get; set; }
        public long TelegramChatId { get; set; }
    }
}