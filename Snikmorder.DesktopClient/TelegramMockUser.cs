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
        public TelegramMockUser(int userId, GameHostService gameHostService)
        {
            _gameHostService = gameHostService;
            UserId = userId;
        }

        public int UserId { get; set; }


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

        public void AddImage(string imagePath)
        {
            Messages.Add(new TelegramImageMockMessage(imagePath, false));
        }

        #region SendMessageCommand

        private RelayCommand _sendMessageCommand;
        private GameHostService _gameHostService;

        public RelayCommand SendMessageCommand
        {
            get { return _sendMessageCommand ??= new RelayCommand(OnExecuteSendMessageCommand, OnCanExecuteSendMessageCommand); }
        }

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

        public RelayCommand SendImageCommand
        {
            get { return _sendImageCommand ?? (_sendImageCommand = new RelayCommand(OnExecuteSendImageCommand)); }
        }

        private void OnExecuteSendImageCommand(object o)
        {
            Messages.Add(new TelegramImageMockMessage(@"C:\Users\akkv\OneDrive\Development\IconExperience\iconex_g2\g_collection\g_collection_png\blue\128x128\astrologer.png", true));
            _gameHostService.SendMessage(UserId, imagePath: @"C:\Users\akkv\OneDrive\Development\IconExperience\iconex_g2\g_collection\g_collection_png\blue\128x128\astrologer.png");
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

        public TelegramMockMessage(string message, bool mine)
        {
            Message = message;
            Mine = mine;
        }
    }

    public class TelegramImageMockMessage : TelegramMockMessage
    {
        public string ImagePath { get; set; }

        public TelegramImageMockMessage(string imagePath, bool mine) : base("", mine)
        {
            ImagePath = imagePath;
        }
    }
}