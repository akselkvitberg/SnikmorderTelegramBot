using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snikmorder.Core.Models;
using Snikmorder.Core.Services;
using Telegram.Bot.Types;

namespace Snikmorder.DesktopClient.GameHost
{
    public class GameHostService
    {
        private readonly MessageHandler MessageHandler;

        public ObservableCollection<TelegramMockUser> Users { get; } = new ObservableCollection<TelegramMockUser>();

        public GameHostService()
        {
            MessageHandler = new MessageHandler(new AdminService(), new PlayerStateMachine(new MockTelegramSender(this), new PlayerRepository()));

            for (var i = 1; i <= 5; i++)
            {
                Users.Add(new TelegramMockUser(i, this));
            }
        }

        internal void SendMessage(int userId, string text = null, string imagePath = null)
        {
            var msg = new Message()
            {
                From = new User()
                {
                    Id = userId
                },
                Chat = new Chat()
                {
                    Id = userId,
                },
                Text = text,
            };

            if (imagePath != null)
            {
                msg.Photo = new[]
                {
                    new PhotoSize()
                    {
                        Height = 10,
                        Width = 10,
                        FileId = imagePath,
                    },
                };
            }

            MessageHandler.OnMessage(msg);
        }
    }

    public class MockTelegramSender : ITelegramSender
    {
        private readonly GameHostService _gameHostService;

        public MockTelegramSender(GameHostService gameHostService)
        {
            _gameHostService = gameHostService;
        }

        public void SendMessage(Player player, string message)
        {
            var telegramMockUser = _gameHostService.Users.FirstOrDefault(x=>x.UserId == player.TelegramUserId);
            telegramMockUser?.AddMessage(message);
        }
    }
}
