using Telegram.Bot.Types;

namespace Snikmorder.Core.Services
{
    public class TelegramPlayerMessage
    {
        public TelegramPlayerMessage(Message message)
        {
            Id = message.From.Id;
            Text = message.Text;
        }

        public TelegramPlayerMessage(int id, string text)
        {
            Id = id;
            Text = text;
        }
        
        public int Id { get; }
        public string Text { get; }
    }
}