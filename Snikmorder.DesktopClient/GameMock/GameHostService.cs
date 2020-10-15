using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Snikmorder.Core.Models;
using Snikmorder.Core.Services;
using Telegram.Bot.Types;

namespace Snikmorder.DesktopClient.GameMock
{
    public class GameHostService
    {
        private MockTelegramSender _mockTelegramSender;

        public ObservableCollection<TelegramMockUser> Users { get; } = new ObservableCollection<TelegramMockUser>();

        public GameHostService()
        {
            _mockTelegramSender = new MockTelegramSender(this);
        }

        public async Task Start()
        {
            //var playerContext = new GameContext(true);
            //await playerContext.Database.EnsureDeletedAsync();
            //await playerContext.Database.EnsureCreatedAsync();

            for (var i = 0; i < 10; i++)
            {
                Users.Add(new TelegramMockUser(i, this, i == 0));
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

            //var repository = new GameRepository(new GameContext(true));
            var repository = new MockGameRepository();
            var gameService = new GameService(repository, _mockTelegramSender);
            var adminStateMachine = new AdminStateMachine(_mockTelegramSender, repository, gameService);
            var playerStateMachine = new PlayerStateMachine(_mockTelegramSender, repository, adminStateMachine, gameService);
            var _messageHandler = new MessageHandler(adminStateMachine, playerStateMachine);
            _ = _messageHandler.OnMessage(msg);

        }
    }

    public class MockTelegramSender : ITelegramSender
    {
        private readonly GameHostService _gameHostService;

        public MockTelegramSender(GameHostService gameHostService)
        {
            _gameHostService = gameHostService;
        }

        public Task SendMessage(int id, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var telegramMockUser = _gameHostService.Users.FirstOrDefault(x=>x.UserId == id);
                telegramMockUser?.AddMessage(message);
            });
            return Task.CompletedTask;
        }

        public Task SendImage(int id, string message, string? pictureId)
        {
            var telegramMockUser = _gameHostService.Users.FirstOrDefault(x=>x.UserId == id);
            telegramMockUser?.AddImage(message, pictureId);
            return Task.CompletedTask;

        }
    }
}
