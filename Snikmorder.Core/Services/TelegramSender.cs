﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

        public Task SendMessage(long id, string message) => SendMessage(new TelegramReplyMessage(id, message));
        public async Task SendImage(long id, string message, string? pictureId)
        {
            _logger.LogDebug($"Sending image: {id}: {pictureId}");
            try
            {
                await _botClient.SendPhotoAsync(id, new InputMedia(pictureId), message);
            }
            catch (Exception )
            {
                
            }
        }

        public async Task SendMessage(TelegramReplyMessage replyMessage)
        {
            var replyMessageText = replyMessage.Text
                .Replace("_", "\\_")
                .Replace("*", "\\*")
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("~", "\\~")
                .Replace("`", "\\`")
                .Replace(">", "\\>")
                .Replace("#", "\\#")
                .Replace("+", "\\+")
                .Replace("-", "\\-")
                .Replace("=", "\\=")
                .Replace("|", "\\|")
                .Replace("{", "\\{")
                .Replace("}", "\\}")
                .Replace(".", "\\.")
                .Replace("!", "\\!");
            _logger.LogDebug($"Sending message: {replyMessage.Id}: {replyMessageText}");
            try
            {
                await _botClient.SendTextMessageAsync(replyMessage.Id, replyMessageText, ParseMode.MarkdownV2, replyMarkup: replyMessage.Keyboard);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }
}