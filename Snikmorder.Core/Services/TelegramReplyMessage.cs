﻿using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Snikmorder.Core.Services
{
    public class TelegramReplyMessage
    {
        public TelegramReplyMessage(Message message)
        {
            Id = message.From.Id;
            Text = message.Text;
            Keyboard = new ReplyKeyboardRemove();
        }

        public TelegramReplyMessage(long id, string text) : this(id, text, new ReplyKeyboardRemove()) { }

        public TelegramReplyMessage(long id, string text, IReplyMarkup keyboard)
        {
            Id = id;
            Text = text;
            Keyboard = keyboard ?? new ReplyKeyboardRemove();
        }

        public long Id { get; }
        public string Text { get; }
        public IReplyMarkup Keyboard { get; }
    }
}