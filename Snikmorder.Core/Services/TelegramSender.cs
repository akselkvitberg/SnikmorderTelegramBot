using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snikmorder.Core.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Snikmorder.Core.Services
{
    public class TelegramSender : ITelegramSender
    {
        private readonly ILogger<TelegramSender> _logger;
        private readonly ITelegramBotClient _botClient;

        public TelegramSender(ILogger<TelegramSender> logger, ITelegramBotClient botClient)
        {
            _logger = logger;
            _botClient = botClient;
        }

        public void SendMessage(int id, string message) => SendMessage(new TelegramReplyMessage(id, message));
        public void SendImage(int id, string message, string? pictureId)
        {
            _logger.LogDebug($"Sending image: {id}: {pictureId}");
            _botClient.SendPhotoAsync(id, new InputMedia(pictureId), message);
        }

        public void SendMessage(Player player, string message) => SendMessage(new TelegramReplyMessage(player.TelegramUserId, message));

        public Task SendMessage(TelegramReplyMessage replyMessage)
        {
            _logger.LogDebug($"Sending message: {replyMessage.Id}: {replyMessage.Text}");
            return _botClient.SendTextMessageAsync(replyMessage.Id, replyMessage.Text, ParseMode.MarkdownV2, replyMarkup: replyMessage.Keyboard);
        }
    }
}