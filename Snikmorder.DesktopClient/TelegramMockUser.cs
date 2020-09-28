using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Snikmorder.DesktopClient.Annotations;
using Snikmorder.DesktopClient.GameHost;
using Telegram.Bot.Types;

namespace Snikmorder.DesktopClient
{
    public class TelegramMockUser : INotifyPropertyChanged
    {
        public TelegramMockUser(int userId, GameHostService gameHostService, bool isAdmin)
        {
            _gameHostService = gameHostService;
            UserId = userId;
            IsAdmin = isAdmin;
            InputText = IsAdmin ? "/neste" : "/start";
        }

        public int UserId { get; set; }
        public bool IsAdmin { get; }
        public string AdminText => IsAdmin ? "Adm" : "";


        public ObservableCollection<TelegramMockMessage> Messages { get; } = new ObservableCollection<TelegramMockMessage>();

        private string _inputText = "/start";

        public string InputText
        {
            get { return _inputText; }
            set
            {
                if (value != _inputText)
                {
                    _inputText = value;
                    OnPropertyChanged();
                }
            }
        }

        public void AddMessage(string message)
        {
            Messages.Add(new TelegramMockMessage(message, false));
        }

        public void AddImage(string message, string imagePath)
        {
            Messages.Add(new TelegramMockMessage(message, false, imagePath));
        }

        #region SendMessageCommand

        private RelayCommand _sendMessageCommand;
        private readonly GameHostService _gameHostService;

        public RelayCommand SendMessageCommand => _sendMessageCommand ??= new RelayCommand(OnExecuteSendMessageCommand, OnCanExecuteSendMessageCommand);

        private void OnExecuteSendMessageCommand(object o)
        {
            Messages.Add(new TelegramMockMessage(InputText, true));
            _gameHostService.SendMessage(UserId, InputText);
            InputText = "";
        }

        private bool OnCanExecuteSendMessageCommand(object o)
        {
            return !string.IsNullOrWhiteSpace(InputText);
        }

        #endregion

        #region SendImageCommand

        private RelayCommand _sendImageCommand;

        public RelayCommand SendImageCommand => _sendImageCommand ??= new RelayCommand(OnExecuteSendImageCommand);

        private void OnExecuteSendImageCommand(object o)
        {
            var imageSource = $"https://api.adorable.io/avatars/128/Agent{UserId}.png";
            Messages.Add(new TelegramMockMessage("", true, imageSource));
            _gameHostService.SendMessage(UserId, imagePath: imageSource);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TelegramMockMessage 
    {
        public string Message { get; }
        public bool Mine { get; }
        public string ImagePath { get; set; }
        public bool ShowImage => ImagePath != null;

        public TelegramMockMessage(string message, bool mine, string imagePath = null)
        {
            Message = message;
            Mine = mine;
            ImagePath = imagePath;
        }
    }
}