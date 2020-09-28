using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snikmorder.Core.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Snikmorder.Core.Services
{
    public class TelegramSender
    {
        private readonly ILogger<TelegramSender> _logger;
        private readonly ITelegramBotClient _botClient;

        public TelegramSender(ILogger<TelegramSender> logger, ITelegramBotClient botClient)
        {
            _logger = logger;
            _botClient = botClient;
        }

        public void SendMessage(int id, string message) => SendMessage(new TelegramReplyMessage(id, message));

        public void SendMessage(Player player, string message) =>  SendMessage(new TelegramReplyMessage(player.TelegramUserId, message));

        public Task SendMessage(TelegramReplyMessage replyMessage)
        {
            _logger.LogDebug($"Sending message: {replyMessage.Id}: {replyMessage.Text}");
            return _botClient.SendTextMessageAsync(replyMessage.Id, replyMessage.Text, ParseMode.MarkdownV2, replyMarkup: replyMessage.Keyboard);
        }
    }
    
    public class TelegramReplyMessage
    {
        public TelegramReplyMessage(Message message)
        {
            Id = message.From.Id;
            Text = message.Text;
            Keyboard = new ReplyKeyboardRemove();
        }

        public TelegramReplyMessage(int id, string text) : this(id, text, new ReplyKeyboardRemove()) { }

        public TelegramReplyMessage(int id, string text, IReplyMarkup keyboard)
        {
            Id = id;
            Text = text;
            Keyboard = keyboard ?? new ReplyKeyboardRemove();
        }

        public int Id { get; }
        public string Text { get; }
        public IReplyMarkup Keyboard { get; }
    }
    
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